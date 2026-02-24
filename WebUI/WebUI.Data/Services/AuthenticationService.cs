using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebUI.Data.Models.Auth;
using WebUI.Core.Security;
using WebUI.Data;
using WebUI.Data.Entities;

namespace WebUI.Data.Services
{
    /// <summary>
    /// Authentication service implementing login, logout, refresh token, and password management
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly WebUIDbContext _dbContext;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IPasswordValidator _passwordValidator;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<AuthenticationService> _logger;

        // Login lockout settings
        private const int MaxFailedLoginAttempts = 5;
        private const int LockoutMinutes = 30;

        public AuthenticationService(
            WebUIDbContext dbContext,
            IPasswordHasher passwordHasher,
            IPasswordValidator passwordValidator,
            IJwtTokenService jwtTokenService,
            IAuditLogService auditLogService,
            ILogger<AuthenticationService> logger)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
            _passwordValidator = passwordValidator;
            _jwtTokenService = jwtTokenService;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<AuthResult> LoginAsync(LoginRequest request)
        {
            try
            {
                // Find user by username
                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Username == request.Username);

                if (user == null)
                {
                    _logger.LogWarning("Login failed: User {Username} not found", request.Username);
                    await _auditLogService.LogAsync(null, "Login", "Failed", $"User {request.Username} not found");
                    return AuthResult.Failure("Invalid username or password");
                }

                // Check if user is locked out
                if (user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow)
                {
                    var lockoutRemaining = user.LockedUntil.Value - DateTime.UtcNow;
                    _logger.LogWarning("Login failed: User {Username} is locked out until {LockedUntil}", 
                        request.Username, user.LockedUntil.Value);
                    await _auditLogService.LogAsync(user.Id, "Login", "Failed", 
                        $"Account locked out. Remaining: {lockoutRemaining.TotalMinutes:F0} minutes");
                    return AuthResult.Failure($"Account is locked. Please try again in {lockoutRemaining.TotalMinutes:F0} minutes");
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    _logger.LogWarning("Login failed: User {Username} is inactive", request.Username);
                    await _auditLogService.LogAsync(user.Id, "Login", "Failed", "Account is inactive");
                    return AuthResult.Failure("Account is inactive");
                }

                // Verify password
                if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
                {
                    // Increment failed login attempts
                    user.FailedLoginAttempts++;

                    // Lock account if max attempts reached
                    if (user.FailedLoginAttempts >= MaxFailedLoginAttempts)
                    {
                        user.LockedUntil = DateTime.UtcNow.AddMinutes(LockoutMinutes);
                        _logger.LogWarning("User {Username} locked out after {Attempts} failed attempts", 
                            request.Username, user.FailedLoginAttempts);
                        await _auditLogService.LogAsync(user.Id, "Login", "Failed", 
                            $"Account locked after {MaxFailedLoginAttempts} failed attempts");
                    }
                    else
                    {
                        _logger.LogWarning("Login failed: Invalid password for user {Username}. Attempt {Attempt}/{Max}", 
                            request.Username, user.FailedLoginAttempts, MaxFailedLoginAttempts);
                        await _auditLogService.LogAsync(user.Id, "Login", "Failed", 
                            $"Invalid password. Attempt {user.FailedLoginAttempts}/{MaxFailedLoginAttempts}");
                    }

                    await _dbContext.SaveChangesAsync();
                    return AuthResult.Failure("Invalid username or password");
                }

                // Reset failed login attempts on successful login
                user.FailedLoginAttempts = 0;
                user.LockedUntil = null;
                user.LastLoginAt = DateTime.UtcNow;

                // Generate tokens
                var tokenResult = _jwtTokenService.GenerateTokens(user.Id, user.Username);

                // Store refresh token
                user.RefreshToken = tokenResult.RefreshToken;
                user.RefreshTokenExpiresAt = tokenResult.RefreshTokenExpiresAt;

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("User {Username} logged in successfully", request.Username);
                await _auditLogService.LogAsync(user.Id, "Login", "Success", "User logged in");

                var response = new LoginResponse
                {
                    AccessToken = tokenResult.AccessToken,
                    RefreshToken = tokenResult.RefreshToken,
                    ExpiresAt = tokenResult.ExpiresAt,
                    UserId = user.Id,
                    Username = user.Username
                };

                return AuthResult.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", request.Username);
                return AuthResult.Failure("An error occurred during login");
            }
        }

        /// <inheritdoc/>
        public async Task<AuthResult> LogoutAsync(int userId)
        {
            try
            {
                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null)
                {
                    return AuthResult.Failure("User not found");
                }

                // Revoke refresh token
                user.RefreshToken = null;
                user.RefreshTokenExpiresAt = null;

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("User {Username} logged out successfully", user.Username);
                await _auditLogService.LogAsync(user.Id, "Logout", "Success", "User logged out");

                return AuthResult.Success(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user {UserId}", userId);
                return AuthResult.Failure("An error occurred during logout");
            }
        }

        /// <inheritdoc/>
        public async Task<AuthResult> RefreshTokenAsync(RefreshTokenRequest request)
        {
            try
            {
                var user = await ValidateRefreshTokenAsync(request.RefreshToken);
                if (user == null)
                {
                    _logger.LogWarning("Refresh token validation failed");
                    return AuthResult.Failure("Invalid or expired refresh token");
                }

                // Generate new tokens
                var tokenResult = _jwtTokenService.GenerateTokens(user.Id, user.Username);

                // Update refresh token (token rotation)
                user.RefreshToken = tokenResult.RefreshToken;
                user.RefreshTokenExpiresAt = tokenResult.RefreshTokenExpiresAt;

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Tokens refreshed for user {Username}", user.Username);
                await _auditLogService.LogAsync(user.Id, "RefreshToken", "Success", "Tokens refreshed");

                var response = new LoginResponse
                {
                    AccessToken = tokenResult.AccessToken,
                    RefreshToken = tokenResult.RefreshToken,
                    ExpiresAt = tokenResult.ExpiresAt,
                    UserId = user.Id,
                    Username = user.Username
                };

                return AuthResult.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return AuthResult.Failure("An error occurred during token refresh");
            }
        }

        /// <inheritdoc/>
        public async Task<AuthResult> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            try
            {
                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null)
                {
                    return AuthResult.Failure("User not found");
                }

                // Verify current password
                if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
                {
                    _logger.LogWarning("Password change failed: Invalid current password for user {Username}", user.Username);
                    await _auditLogService.LogAsync(user.Id, "ChangePassword", "Failed", "Invalid current password");
                    return AuthResult.Failure("Current password is incorrect");
                }

                // Validate new password
                var validationResult = _passwordValidator.Validate(request.NewPassword);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Password change failed: {Error} for user {Username}", 
                        validationResult.ErrorMessage, user.Username);
                    return AuthResult.Failure(validationResult.ErrorMessage!);
                }

                // Check if new password is same as current
                if (_passwordHasher.VerifyPassword(request.NewPassword, user.PasswordHash))
                {
                    return AuthResult.Failure("New password must be different from current password");
                }

                // Hash and update password
                user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);

                // Revoke all existing refresh tokens for security
                user.RefreshToken = null;
                user.RefreshTokenExpiresAt = null;

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Password changed successfully for user {Username}", user.Username);
                await _auditLogService.LogAsync(user.Id, "ChangePassword", "Success", "Password changed");

                return AuthResult.Success(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change for user {UserId}", userId);
                return AuthResult.Failure("An error occurred during password change");
            }
        }

        /// <inheritdoc/>
        public async Task<User?> ValidateRefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return null;
            }

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (user == null)
            {
                return null;
            }

            // Check if refresh token is expired
            if (!user.RefreshTokenExpiresAt.HasValue || user.RefreshTokenExpiresAt.Value < DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh token expired for user {Username}", user.Username);
                return null;
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Refresh token validation failed: User {Username} is inactive", user.Username);
                return null;
            }

            return user;
        }
    }
}
