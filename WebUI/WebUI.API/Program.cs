using System.Text;
using Asp.Versioning;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using WebUI.API.Services;
using WebUI.Core.Configuration;
using WebUI.Core.Health;
using WebUI.Core.Middleware;
using WebUI.Core.Security;
using WebUI.Core.Services;
using WebUI.Data;
using WebUI.Data.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/webui-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// Configure FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Configure SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaximumReceiveMessageSize = 1024 * 1024; // 1 MB
    options.StreamBufferCapacity = 10;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// Configure API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Configure JWT settings
var jwtSecretKey = builder.Configuration["WebUI:JwtSecret"];
if (string.IsNullOrWhiteSpace(jwtSecretKey) || jwtSecretKey.Length < 32)
{
    throw new InvalidOperationException("JWT SecretKey must be configured and at least 32 characters long");
}

builder.Services.Configure<JwtSettings>(options =>
{
    options.SecretKey = jwtSecretKey;
    options.Issuer = builder.Configuration["WebUI:JwtIssuer"] ?? "WebUI.API";
    options.Audience = builder.Configuration["WebUI:JwtAudience"] ?? "WebUI.Client";
    
    if (int.TryParse(builder.Configuration["WebUI:JwtExpirationMinutes"], out var expMinutes))
        options.AccessTokenExpirationMinutes = expMinutes;
    
    if (int.TryParse(builder.Configuration["WebUI:RefreshTokenExpirationDays"], out var expDays))
        options.RefreshTokenExpirationDays = expDays;
});

// Configure JWT authentication
var key = Encoding.UTF8.GetBytes(jwtSecretKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["WebUI:JwtIssuer"],
        ValidAudience = builder.Configuration["WebUI:JwtAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.FromMinutes(5)
    };

    // Configure JWT authentication for SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Allow SignalR to receive JWT token from query string
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Configure CORS
var allowedOrigins = builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>() 
    ?? new[] { "http://localhost:3000", "http://localhost:5173", "http://localhost:5174" };

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure database
var dbProvider = builder.Configuration["WebUI:DatabaseProvider"] ?? "SQLite";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Database connection string is required");
}

builder.Services.AddDbContext<WebUIDbContext>(options =>
{
    if (dbProvider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
    {
        options.UseNpgsql(connectionString);
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});

// Register services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IPasswordValidator, PasswordValidator>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// Register data access services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register repository services
builder.Services.AddScoped<WebUI.Data.Repositories.IPositionRepository, WebUI.Data.Repositories.PositionRepository>();

// Register trading service
builder.Services.AddScoped<ITradingService, TradingService>();

// Register strategy service
builder.Services.AddScoped<IStrategyService, StrategyService>();

// Register strategy execution service
builder.Services.AddScoped<IStrategyExecutionService, StrategyExecutionService>();

// Register strategy log streaming service (singleton to maintain state)
builder.Services.AddSingleton<IStrategyLogStreamingService, StrategyLogStreamingService>();

// Register backtest service
builder.Services.AddScoped<IBacktestService, BacktestService>();

// Register backtest execution service
builder.Services.AddScoped<IBacktestExecutionService, BacktestExecutionService>();

// Register parameter optimization service
builder.Services.AddScoped<IParameterOptimizationService, ParameterOptimizationService>();

// Register backtest export service
builder.Services.AddScoped<IBacktestExportService, BacktestExportService>();

// Register portfolio service
builder.Services.AddScoped<IPortfolioService, PortfolioService>();

// Register risk management service
builder.Services.AddScoped<IRiskService, RiskService>();

// Register ETF service
builder.Services.AddScoped<IEtfService, EtfService>();

// Configure cache service (Redis or in-memory based on configuration)
var useRedis = builder.Configuration.GetValue<bool>("WebUI:UseRedisCache");
var redisConnection = builder.Configuration.GetConnectionString("Redis");

if (useRedis && !string.IsNullOrWhiteSpace(redisConnection))
{
    builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(
        StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnection));
    builder.Services.AddSingleton<ICacheService, RedisCacheService>();
    Log.Information("Using Redis cache: {ConnectionString}", redisConnection);
}
else
{
    builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
    Log.Information("Using in-memory cache");
}

// Register IBKR and market data services
// IbkrConnectionService extends BackgroundService; register the concrete type as a singleton
// so it can be injected via IIbkrConnectionService, AND as a hosted service so ExecuteAsync starts.
builder.Services.AddSingleton<IbkrConnectionService>();
builder.Services.AddSingleton<IIbkrConnectionService>(sp => sp.GetRequiredService<IbkrConnectionService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<IbkrConnectionService>());
builder.Services.AddSingleton<IMarketDataService, MarketDataService>();

// Register IBKR TWS API client and position sync service
builder.Services.AddSingleton<IIbkrTwsApiClient, IbkrTwsApiClient>();
builder.Services.AddScoped<IIbkrPositionSyncService, IbkrPositionSyncService>();

// Register SignalR push services
builder.Services.AddSingleton<IMarketDataPushService, WebUI.API.Services.MarketDataPushService>();
builder.Services.AddSingleton<IOrderPushService, WebUI.API.Services.OrderPushService>();
builder.Services.AddSingleton<IStrategyPushService, WebUI.API.Services.StrategyPushService>();

// Add background services
builder.Services.AddHostedService<WebUI.API.Services.StrategyLogFlushService>();
builder.Services.AddHostedService<WebUI.API.Services.RecurringPlanExecutionService>();
builder.Services.AddHostedService<GracefulShutdownService>();

// Configure graceful shutdown timeout (default is 30 seconds)
builder.Services.Configure<HostOptions>(options =>
{
    options.ShutdownTimeout = TimeSpan.FromSeconds(30);
});

// Configure Swagger/OpenAPI
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

// Configure response compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Configure HSTS for production
if (!builder.Environment.IsDevelopment())
{
    var hstsMaxAge = builder.Configuration.GetValue<int>("Security:HSTSMaxAge", 31536000); // 1 year
    builder.Services.AddHsts(options =>
    {
        options.MaxAge = TimeSpan.FromSeconds(hstsMaxAge);
        options.IncludeSubDomains = true;
        options.Preload = true;
    });
}

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<WebUIDbContext>("database")
    .AddCheck<DiskSpaceHealthCheck>("disk_space", tags: new[] { "detailed" })
    .AddCheck<MemoryHealthCheck>("memory", tags: new[] { "detailed" })
    .AddCheck<ReadinessHealthCheck>("readiness", tags: new[] { "ready" });

var app = builder.Build();

// Wire up log streaming service to execution service (to avoid circular dependency)
using (var scope = app.Services.CreateScope())
{
    var executionService = scope.ServiceProvider.GetService<IStrategyExecutionService>() as StrategyExecutionService;
    var logStreamingService = scope.ServiceProvider.GetService<WebUI.API.Services.IStrategyLogStreamingService>();
    
    if (executionService != null && logStreamingService != null)
    {
        executionService.SetLogStreamingService(logStreamingService);
        Log.Information("Wired up log streaming service to strategy execution service");
    }
}

// Configure the HTTP request pipeline
app.UseGlobalExceptionHandler();

// CORS must be called early in the pipeline, before routing
app.UseCors();

app.UseRateLimiting();
app.UseSerilogRequestLogging();

// Configure HSTS in production
if (!app.Environment.IsDevelopment())
{
    var enableHSTS = builder.Configuration.GetValue<bool>("Security:EnableHSTS", true);
    if (enableHSTS)
    {
        var hstsMaxAge = builder.Configuration.GetValue<int>("Security:HSTSMaxAge", 31536000); // 1 year default
        app.UseHsts();
        Log.Information("HSTS enabled with max-age: {MaxAge} seconds", hstsMaxAge);
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("WebUI API 文档 / WebUI API Documentation")
            .WithTheme(ScalarTheme.Purple)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

// Always redirect to HTTPS (can be disabled in development if needed)
var requireHttps = builder.Configuration.GetValue<bool>("Security:EnableHTTPSRedirect", true);
if (requireHttps)
{
    app.UseHttpsRedirection();
}

// Add security headers
app.UseSecurityHeaders();

app.UseResponseCompression();

// Serve static files from wwwroot (if hosting frontend from backend)
var serveStaticFiles = builder.Configuration.GetValue<bool>("WebUI:ServeStaticFiles", false);
if (serveStaticFiles)
{
    app.UseDefaultFiles();
    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            // Cache static assets for 1 year
            if (ctx.File.Name.EndsWith(".js") || ctx.File.Name.EndsWith(".css") ||
                ctx.File.Name.EndsWith(".woff") || ctx.File.Name.EndsWith(".woff2") ||
                ctx.File.Name.EndsWith(".ttf") || ctx.File.Name.EndsWith(".eot") ||
                ctx.File.Name.EndsWith(".png") || ctx.File.Name.EndsWith(".jpg") ||
                ctx.File.Name.EndsWith(".svg") || ctx.File.Name.EndsWith(".ico"))
            {
                ctx.Context.Response.Headers["Cache-Control"] = "public,max-age=31536000,immutable";
            }
            // Don't cache index.html
            else if (ctx.File.Name.Equals("index.html", StringComparison.OrdinalIgnoreCase))
            {
                ctx.Context.Response.Headers["Cache-Control"] = "no-cache,no-store,must-revalidate";
            }
        }
    });
    
    Log.Information("Static file serving enabled from wwwroot/");
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map SignalR hubs
app.MapHub<WebUI.API.Hubs.MarketDataHub>("/hubs/marketdata");
app.MapHub<WebUI.API.Hubs.OrderHub>("/hubs/orders");
app.MapHub<WebUI.API.Hubs.StrategyHub>("/hubs/strategies");

Log.Information("SignalR hubs mapped: /hubs/marketdata, /hubs/orders, /hubs/strategies");

// Map health check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow
        });
        await context.Response.WriteAsync(result);
    }
});

app.MapHealthChecks("/health/detailed", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            duration = report.TotalDuration,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration,
                data = e.Value.Data
            })
        }, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
        await context.Response.WriteAsync(result);
    }
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            ready = report.Status == Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy
        });
        await context.Response.WriteAsync(result);
    }
});

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<WebUIDbContext>();
    try
    {
        if (dbProvider.Equals("SQLite", StringComparison.OrdinalIgnoreCase))
        {
            dbContext.Database.EnsureCreated();
            // Ensure new tables added after initial EnsureCreated are present
            dbContext.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS SystemSettings (
                    Key TEXT NOT NULL PRIMARY KEY,
                    Value TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL
                )");
        }
        else
        {
            dbContext.Database.Migrate();
        }
        
        // Seed default data
        await DatabaseConfiguration.SeedDefaultDataAsync(dbContext);
        
        Log.Information("Database initialized successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error initializing database");
        throw;
    }
}

// SPA fallback - serve index.html for all unmatched routes (if serving static files)
if (serveStaticFiles)
{
    app.MapFallbackToFile("index.html");
    Log.Information("SPA fallback configured - serving index.html for unmatched routes");
}

Log.Information("Starting WebUI API...");

app.Run();
