using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using WebUI.Data.Models.Auth;
using WebUI.Data.Models.Trading;
using Xunit;

namespace WebUI.Tests.Integration
{
    /// <summary>
    /// 控制器集成测试 - 测试所有主要API端点
    /// </summary>
    public class ControllersIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ControllersIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        #region Auth Controller Tests

        [Fact]
        public async Task AuthLogin_WithInvalidCredentials_ShouldReturnUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "nonexistent",
                Password = "wrongpassword"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task AuthLogin_WithoutCredentials_ShouldReturnBadRequest()
        {
            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new { });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion

        #region Market Controller Tests

        [Fact]
        public async Task MarketSummary_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/market/summary");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task MarketQuote_WithInvalidSymbol_ShouldReturnBadRequest()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/market/quote/");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        #endregion

        #region IBKR Controller Tests

        [Fact]
        public async Task IbkrAccount_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/ibkr/account");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task IbkrDiagnostics_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/ibkr/diagnostics");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Orders Controller Tests

        [Fact]
        public async Task OrdersList_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/orders");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task OrderCreate_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Arrange
            var orderRequest = new CreateOrderRequest
            {
                Symbol = "AAPL",
                Quantity = 100,
                Side = "Buy",
                Type = "Market"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/orders", orderRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Strategies Controller Tests

        [Fact]
        public async Task StrategiesList_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/strategies");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task StrategyStart_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.PostAsync($"/api/v1/strategies/{Guid.NewGuid()}/start", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Backtests Controller Tests

        [Fact]
        public async Task BacktestsList_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/backtests");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task BacktestCreate_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/backtests", new { });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Portfolio Controller Tests

        [Fact]
        public async Task PortfolioSummary_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/portfolio/summary");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Positions Controller Tests

        [Fact]
        public async Task PositionsList_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/positions");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Risk Controller Tests

        [Fact]
        public async Task RiskConfig_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/risk/config");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task RiskReport_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/risk/report");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region ETFs Controller Tests

        [Fact]
        public async Task EtfsList_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/etfs");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task EtfSearch_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/etfs/search?query=SPY");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region General API Tests

        [Fact]
        public async Task ApiEndpoints_InvalidRoute_ShouldReturn404()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/nonexistent");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ApiEndpoints_WithInvalidContentType_ShouldReturnUnsupportedMediaType()
        {
            // Arrange
            var content = new StringContent("plain text", System.Text.Encoding.UTF8, "text plain");

            // Act
            var response = await _client.PostAsync("/api/v1/auth/login", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.UnsupportedMediaType, HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("/api/v1/orders")]
        [InlineData("/api/v1/strategies")]
        [InlineData("/api/v1/positions")]
        [InlineData("/api/v1/portfolio/summary")]
        public async Task ProtectedEndpoints_WithoutAuth_ShouldReturn401(string endpoint)
        {
            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion
    }
}
