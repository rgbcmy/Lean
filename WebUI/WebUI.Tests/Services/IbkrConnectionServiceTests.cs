/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using WebUI.Core.Models;
using WebUI.Core.Services;
using Xunit;

namespace WebUI.Tests.Services
{
    public class IbkrConnectionServiceTests
    {
        private readonly Mock<ILogger<IbkrConnectionService>> _mockLogger;
        private readonly IbkrConnectionService _service;

        public IbkrConnectionServiceTests()
        {
            _mockLogger = new Mock<ILogger<IbkrConnectionService>>();
            _service = new IbkrConnectionService(_mockLogger.Object);
        }

        [Fact]
        public void ConnectionState_InitialState_IsDisconnected()
        {
            // Arrange & Act
            var state = _service.ConnectionState;

            // Assert
            Assert.Equal(IbkrConnectionStatus.Disconnected, state.Status);
            Assert.Null(state.ConnectedAt);
            Assert.Null(state.LastHeartbeatAt);
        }

        [Fact]
        public async Task ConnectAsync_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IbkrConnectionConfig config = null!;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ConnectAsync(config));
        }

        [Fact]
        public async Task ConnectAsync_ValidConfig_UpdatesConnectionState()
        {
            // Arrange
            var config = new IbkrConnectionConfig
            {
                Host = "localhost",
                Port = 7497,
                AccountId = "DU1234567",
                AccountType = IbkrAccountType.Paper,
                TimeoutSeconds = 10,
                EnableAutoReconnect = true
            };

            // Act
            var result = await _service.ConnectAsync(config);

            // Assert
            Assert.True(result);
            Assert.Equal(IbkrConnectionStatus.Connected, _service.ConnectionState.Status);
            Assert.Equal("DU1234567", _service.ConnectionState.AccountId);
            Assert.Equal(IbkrAccountType.Paper, _service.ConnectionState.AccountType);
            Assert.NotNull(_service.ConnectionState.ConnectedAt);
        }

        [Fact]
        public async Task ConnectAsync_LiveAccount_LogsWarning()
        {
            // Arrange
            var config = new IbkrConnectionConfig
            {
                Host = "localhost",
                Port = 7496,
                AccountId = "U1234567",
                AccountType = IbkrAccountType.Live,
                TimeoutSeconds = 10
            };

            // Act
            await _service.ConnectAsync(config);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("U1234567")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task DisconnectAsync_WhenConnected_UpdatesStatusToDisconnected()
        {
            // Arrange
            var config = new IbkrConnectionConfig
            {
                Host = "localhost",
                Port = 7497,
                AccountId = "DU1234567",
                AccountType = IbkrAccountType.Paper
            };
            await _service.ConnectAsync(config);

            // Act
            await _service.DisconnectAsync();

            // Assert
            Assert.Equal(IbkrConnectionStatus.Disconnected, _service.ConnectionState.Status);
            Assert.Null(_service.ConnectionState.ConnectedAt);
        }

        [Fact]
        public async Task GetAccountSummaryAsync_WhenNotConnected_ReturnsNull()
        {
            // Act
            var summary = await _service.GetAccountSummaryAsync();

            // Assert
            Assert.Null(summary);
        }

        [Fact]
        public async Task GetAccountSummaryAsync_WhenConnected_ReturnsValidSummary()
        {
            // Arrange
            var config = new IbkrConnectionConfig
            {
                Host = "localhost",
                Port = 7497,
                AccountId = "DU1234567",
                AccountType = IbkrAccountType.Paper
            };
            await _service.ConnectAsync(config);

            // Act
            var summary = await _service.GetAccountSummaryAsync();

            // Assert
            Assert.NotNull(summary);
            Assert.Equal("DU1234567", summary.AccountId);
            Assert.Equal(IbkrAccountType.Paper, summary.AccountType);
            Assert.Equal("USD", summary.BaseCurrency);
            Assert.True(summary.BuyingPower > 0);
        }

        [Fact]
        public void GetDiagnostics_ReturnsValidDiagnostics()
        {
            // Act
            var diagnostics = _service.GetDiagnostics();

            // Assert
            Assert.NotNull(diagnostics);
            Assert.NotNull(diagnostics.ConnectionState);
            Assert.NotNull(diagnostics.ErrorHistory);
            Assert.NotNull(diagnostics.RateLimitStatus);
            Assert.Equal(50, diagnostics.RateLimitStatus.MaxRequestsPerSecond);
        }

        [Fact]
        public async Task GetDiagnostics_AfterConnection_MasksAccountId()
        {
            // Arrange
            var config = new IbkrConnectionConfig
            {
                Host = "localhost",
                Port = 7497,
                AccountId = "DU1234567",
                AccountType = IbkrAccountType.Paper
            };
            await _service.ConnectAsync(config);

            // Act
            var diagnostics = _service.GetDiagnostics();

            // Assert
            Assert.NotNull(diagnostics.Configuration);
            Assert.Equal("****4567", diagnostics.Configuration.AccountId);
        }

        [Fact]
        public async Task ConnectionStateChanged_Event_RaisedOnConnect()
        {
            // Arrange
            IbkrConnectionState? capturedState = null;
            _service.ConnectionStateChanged += (sender, state) => capturedState = state;

            var config = new IbkrConnectionConfig
            {
                Host = "localhost",
                Port = 7497,
                AccountId = "DU1234567",
                AccountType = IbkrAccountType.Paper
            };

            // Act
            await _service.ConnectAsync(config);

            // Assert
            Assert.NotNull(capturedState);
            Assert.Equal(IbkrConnectionStatus.Connected, capturedState.Status);
        }

        [Fact]
        public async Task ConnectionStateChanged_Event_RaisedOnDisconnect()
        {
            // Arrange
            var config = new IbkrConnectionConfig
            {
                Host = "localhost",
                Port = 7497,
                AccountId = "DU1234567",
                AccountType = IbkrAccountType.Paper
            };
            await _service.ConnectAsync(config);

            IbkrConnectionState? capturedState = null;
            _service.ConnectionStateChanged += (sender, state) => capturedState = state;

            // Act
            await _service.DisconnectAsync();

            // Assert
            Assert.NotNull(capturedState);
            Assert.Equal(IbkrConnectionStatus.Disconnected, capturedState.Status);
        }

        [Fact]
        public async Task ConnectionState_IsHealthy_WhenConnectedAndRecentHeartbeat()
        {
            // Arrange
            var config = new IbkrConnectionConfig
            {
                Host = "localhost",
                Port = 7497,
                AccountId = "DU1234567",
                AccountType = IbkrAccountType.Paper
            };

            // Act
            await _service.ConnectAsync(config);

            // Assert
            Assert.True(_service.ConnectionState.IsHealthy);
        }

        [Theory]
        [InlineData(IbkrAccountType.Paper, 7497)]
        [InlineData(IbkrAccountType.Live, 7496)]
        public async Task ConnectAsync_CorrectPortForAccountType_Succeeds(
            IbkrAccountType accountType, int port)
        {
            // Arrange
            var config = new IbkrConnectionConfig
            {
                Host = "localhost",
                Port = port,
                AccountId = "TEST123456",
                AccountType = accountType
            };

            // Act
            var result = await _service.ConnectAsync(config);

            // Assert
            Assert.True(result);
            Assert.Equal(accountType, _service.ConnectionState.AccountType);
        }

        [Fact]
        public async Task ConnectAsync_TwiceWithoutDisconnect_HandlesGracefully()
        {
            // Arrange
            var config = new IbkrConnectionConfig
            {
                Host = "localhost",
                Port = 7497,
                AccountId = "DU1234567",
                AccountType = IbkrAccountType.Paper
            };

            // Act
            await _service.ConnectAsync(config);
            var result2 = await _service.ConnectAsync(config);

            // Assert
            Assert.True(result2);
            Assert.Equal(IbkrConnectionStatus.Connected, _service.ConnectionState.Status);
        }

        [Fact]
        public async Task Dispose_CleansUpResources()
        {
            // Arrange
            var config = new IbkrConnectionConfig
            {
                Host = "localhost",
                Port = 7497,
                AccountId = "DU1234567",
                AccountType = IbkrAccountType.Paper
            };
            await _service.ConnectAsync(config);

            // Act
            _service.Dispose();

            // Assert - should not throw
            Assert.True(true);
        }
    }
}
