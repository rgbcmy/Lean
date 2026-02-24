using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Core.Models;
using WebUI.Data.Models.Auth;
using WebUI.Data.Services;

namespace WebUI.API.Controllers
{
    /// <summary>
    /// Authentication controller for login, logout, token refresh, and password management
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authService;

        public AuthController(IAuthenticationService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// User login endpoint
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>JWT tokens and user information</returns>
        /// <response code="200">Login successful</response>
        /// <response code="400">Invalid request</response>
        /// <response code="401">Invalid credentials or account locked</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
                return BadRequest(new ErrorResponse("VALIDATION_ERROR", "Invalid request", errors));
            }

            var result = await _authService.LoginAsync(request);

            if (!result.IsSuccess)
            {
                return Unauthorized(new ErrorResponse("LOGIN_FAILED", result.ErrorMessage!));
            }

            return Ok(result.LoginResponse);
        }

        /// <summary>
        /// User logout endpoint
        /// </summary>
        /// <returns>Success response</returns>
        /// <response code="200">Logout successful</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Logout()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new ErrorResponse("INVALID_USER", "Invalid user context"));
            }

            var result = await _authService.LogoutAsync(userId.Value);

            if (!result.IsSuccess)
            {
                return BadRequest(new ErrorResponse("LOGOUT_FAILED", result.ErrorMessage!));
            }

            return Ok(new { Message = "Logout successful" });
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        /// <param name="request">Refresh token</param>
        /// <returns>New JWT tokens</returns>
        /// <response code="200">Token refresh successful</response>
        /// <response code="400">Invalid refresh token</response>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorResponse.ValidationError("Invalid request", ModelState));
            }

            var result = await _authService.RefreshTokenAsync(request);

            if (!result.IsSuccess)
            {
                return BadRequest(new ErrorResponse("REFRESH_TOKEN_FAILED", result.ErrorMessage!));
            }

            return Ok(result.LoginResponse);
        }

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="request">Password change request</param>
        /// <returns>Success response</returns>
        /// <response code="200">Password changed successfully</response>
        /// <response code="400">Invalid request or password validation failed</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorResponse.ValidationError("Invalid request", ModelState));
            }

            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new ErrorResponse("UNAUTHORIZED", "Invalid user context"));
            }

            var result = await _authService.ChangePasswordAsync(userId.Value, request);

            if (!result.IsSuccess)
            {
                return BadRequest(new ErrorResponse("CHANGE_PASSWORD_FAILED", result.ErrorMessage!));
            }

            return Ok(new { Message = "Password changed successfully. Please login again." });
        }

        /// <summary>
        /// Get current user information
        /// </summary>
        /// <returns>User information</returns>
        /// <response code="200">User information retrieved</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult GetCurrentUser()
        {
            var userId = GetUserId();
            var username = User.Identity?.Name;

            if (!userId.HasValue || string.IsNullOrEmpty(username))
            {
                return Unauthorized(new ErrorResponse("UNAUTHORIZED", "Invalid user context"));
            }

            return Ok(new 
            { 
                UserId = userId.Value,
                Username = username
            });
        }

        private int? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return null;
        }
    }
}
