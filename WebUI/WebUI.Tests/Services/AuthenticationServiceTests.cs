using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using WebUI.Data.Models.Auth;
using WebUI.Core.Security;
using WebUI.Data.Services;
using WebUI.Data;
using WebUI.Data.Entities;
using Xunit;

namespace WebUI.Tests.Services
{
    public class AuthenticationServiceTests : IDisposable
    {
        private readonly WebUIDbContext _dbContext;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<IPasswordValidator> _passwordValidatorMock;
        private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
        private readonly Mock<IAuditLogService> _auditLogServiceMock;
        private readonly Mock<ILogger<AuthenticationService>> _loggerMock;
        private readonly IAuthenticationService _authService;

        public AuthenticationServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<WebUIDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new WebUIDbContext(options);

            // Setup mocks
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _passwordValidatorMock = new Mock<IPasswordValidator>();
            _jwtTokenServiceMock = new Mock<IJwtTokenService>();
            _auditLogServiceMock = new Mock<IAuditLogService>();
            _loggerMock = new Mock<ILogger<AuthenticationService>>();

            _authService = new AuthenticationService(
                _dbContext,
                _passwordHasherMock.Object,
                _passwordValidatorMock.Object,
                _jwtTokenServiceMock.Object,
                _auditLogServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task LoginAsync_ShouldSucceed_WhenCredentialsAreValid()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                PasswordHash = "hashed_password",
                IsActive = true,
                FailedLoginAttempts = 0
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "Password123!"
            };

            _passwordHasherMock.Setup(x => x.VerifyPassword(loginRequest.Password, user.PasswordHash))
                .Returns(true);

            var tokenResult = new TokenResult
            {
                AccessToken = "access_token",
                RefreshToken = "refresh_token",
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
            };
            _jwtTokenServiceMock.Setup(x => x.GenerateTokens(user.Id, user.Username, null))
                .Returns(tokenResult);

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.LoginResponse.Should().NotBeNull();
            result.LoginResponse!.AccessToken.Should().Be("access_token");
            result.LoginResponse.RefreshToken.Should().Be("refresh_token");
            result.LoginResponse.UserId.Should().Be(user.Id);
            result.LoginResponse.Username.Should().Be(user.Username);

            // Verify user was updated
            user.FailedLoginAttempts.Should().Be(0);
            user.LastLoginAt.Should().NotBeNull();
            user.RefreshToken.Should().Be("refresh_token");
        }

        [Fact]
        public async Task LoginAsync_ShouldFail_WhenUserNotFound()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "nonexistent",
                Password = "Password123!"
            };

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Invalid username or password");
        }

        [Fact]
        public async Task LoginAsync_ShouldFail_WhenPasswordIsIncorrect()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                PasswordHash = "hashed_password",
                IsActive = true,
                FailedLoginAttempts = 0
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "WrongPassword123!"
            };

            _passwordHasherMock.Setup(x => x.VerifyPassword(loginRequest.Password, user.PasswordHash))
                .Returns(false);

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Invalid username or password");

            // Verify failed login attempts incremented
            user.FailedLoginAttempts.Should().Be(1);
        }

        [Fact]
        public async Task LoginAsync_ShouldLockAccount_AfterMaxFailedAttempts()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                PasswordHash = "hashed_password",
                IsActive = true,
                FailedLoginAttempts = 4 // One more attempt will lock
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "WrongPassword123!"
            };

            _passwordHasherMock.Setup(x => x.VerifyPassword(loginRequest.Password, user.PasswordHash))
                .Returns(false);

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            result.IsSuccess.Should().BeFalse();
            user.FailedLoginAttempts.Should().Be(5);
            user.LockedUntil.Should().NotBeNull();
            user.LockedUntil.Should().BeAfter(DateTime.UtcNow);
        }

        [Fact]
        public async Task LoginAsync_ShouldFail_WhenAccountIsLocked()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                PasswordHash = "hashed_password",
                IsActive = true,
                FailedLoginAttempts = 5,
                LockedUntil = DateTime.UtcNow.AddMinutes(30)
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "Password123!"
            };

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("locked");
        }

        [Fact]
        public async Task LoginAsync_ShouldFail_WhenAccountIsInactive()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                PasswordHash = "hashed_password",
                IsActive = false
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "Password123!"
            };

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("inactive");
        }

        [Fact]
        public async Task LogoutAsync_ShouldRevokeRefreshToken()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                PasswordHash = "hashed_password",
                RefreshToken = "refresh_token",
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _authService.LogoutAsync(user.Id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            user.RefreshToken.Should().BeNull();
            user.RefreshTokenExpiresAt.Should().BeNull();
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldSucceed_WhenRefreshTokenIsValid()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                PasswordHash = "hashed_password",
                IsActive = true,
                RefreshToken = "valid_refresh_token",
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            var request = new RefreshTokenRequest
            {
                RefreshToken = "valid_refresh_token"
            };

            var newTokenResult = new TokenResult
            {
                AccessToken = "new_access_token",
                RefreshToken = "new_refresh_token",
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
            };
            _jwtTokenServiceMock.Setup(x => x.GenerateTokens(user.Id, user.Username, null))
                .Returns(newTokenResult);

            // Act
            var result = await _authService.RefreshTokenAsync(request);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.LoginResponse!.AccessToken.Should().Be("new_access_token");
            result.LoginResponse.RefreshToken.Should().Be("new_refresh_token");

            // Verify token was rotated
            user.RefreshToken.Should().Be("new_refresh_token");
        }

        [Fact]
        public async Task ChangePasswordAsync_ShouldSucceed_WhenCurrentPasswordIsCorrect()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                PasswordHash = "old_hashed_password"
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            var request = new ChangePasswordRequest
            {
                CurrentPassword = "OldPassword123!",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };

            _passwordHasherMock.Setup(x => x.VerifyPassword(request.CurrentPassword, user.PasswordHash))
                .Returns(true);
            _passwordHasherMock.Setup(x => x.VerifyPassword(request.NewPassword, user.PasswordHash))
                .Returns(false);
            _passwordHasherMock.Setup(x => x.HashPassword(request.NewPassword))
                .Returns("new_hashed_password");
            _passwordValidatorMock.Setup(x => x.Validate(request.NewPassword))
                .Returns(PasswordValidationResult.Success());

            // Act
            var result = await _authService.ChangePasswordAsync(user.Id, request);

            // Assert
            result.IsSuccess.Should().BeTrue();
            user.PasswordHash.Should().Be("new_hashed_password");
            user.RefreshToken.Should().BeNull();
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}
