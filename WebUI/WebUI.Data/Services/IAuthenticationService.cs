using System;
using System.Threading.Tasks;
using WebUI.Data.Models.Auth;
using WebUI.Data.Entities;

namespace WebUI.Data.Services
{
    /// <summary>
    /// Authentication service result
    /// </summary>
    public class AuthResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public User? User { get; set; }
        public LoginResponse? LoginResponse { get; set; }

        public static AuthResult Success(User user) => new AuthResult { IsSuccess = true, User = user };
        public static AuthResult Success(LoginResponse loginResponse) => 
            new AuthResult { IsSuccess = true, LoginResponse = loginResponse };
        public static AuthResult Failure(string errorMessage) => 
            new AuthResult { IsSuccess = false, ErrorMessage = errorMessage };
    }

    /// <summary>
    /// Interface for authentication operations
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Authenticate user and generate tokens
        /// </summary>
        /// <param name="request">Login request</param>
        /// <returns>Authentication result with tokens</returns>
        Task<AuthResult> LoginAsync(LoginRequest request);

        /// <summary>
        /// Logout user and revoke refresh token
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Success or failure</returns>
        Task<AuthResult> LogoutAsync(int userId);

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        /// <param name="request">Refresh token request</param>
        /// <returns>New access and refresh tokens</returns>
        Task<AuthResult> RefreshTokenAsync(RefreshTokenRequest request);

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="request">Change password request</param>
        /// <returns>Success or failure</returns>
        Task<AuthResult> ChangePasswordAsync(int userId, ChangePasswordRequest request);

        /// <summary>
        /// Validate refresh token
        /// </summary>
        /// <param name="refreshToken">Refresh token to validate</param>
        /// <returns>User if valid, null otherwise</returns>
        Task<User?> ValidateRefreshTokenAsync(string refreshToken);
    }
}
