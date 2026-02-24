using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using WebUI.Core.Middleware;
using Xunit;

namespace WebUI.Tests.Middleware
{
    public class GlobalExceptionHandlerMiddlewareTests
    {
        private readonly Mock<ILogger<GlobalExceptionHandlerMiddleware>> _loggerMock;
        private readonly GlobalExceptionHandlerMiddleware _middleware;

        public GlobalExceptionHandlerMiddlewareTests()
        {
            _loggerMock = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
            _middleware = new GlobalExceptionHandlerMiddleware(_loggerMock.Object);
        }

        [Fact]
        public async Task InvokeAsync_NoException_ShouldCallNextDelegate()
        {
            // Arrange
            var nextCalled = false;
            RequestDelegate next = (HttpContext context) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await _middleware.InvokeAsync(context, next);

            // Assert
            nextCalled.Should().BeTrue();
            context.Response.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task InvokeAsync_ExceptionThrown_ShouldReturn500AndLogError()
        {
            // Arrange
            var exception = new Exception("Test exception");
            RequestDelegate next = (HttpContext context) => throw exception;

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await _middleware.InvokeAsync(context, next);

            // Assert
            context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            context.Response.ContentType.Should().Be("application/json");

            // Verify logger was called
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_ExceptionThrown_ShouldReturnJsonErrorResponse()
        {
            // Arrange
            var exception = new Exception("Test exception message");
            RequestDelegate next = (HttpContext context) => throw exception;

            var context = new DefaultHttpContext();
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // Act
            await _middleware.InvokeAsync(context, next);

            // Assert
            responseBody.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(responseBody).ReadToEndAsync();

            responseText.Should().Contain("error");
            responseText.Should().Contain("Test exception message");
        }

        [Fact]
        public async Task InvokeAsync_UnauthorizedAccessException_ShouldReturn403()
        {
            // Arrange
            var exception = new UnauthorizedAccessException("Access denied");
            RequestDelegate next = (HttpContext context) => throw exception;

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await _middleware.InvokeAsync(context, next);

            // Assert
            context.Response.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task InvokeAsync_ArgumentException_ShouldReturn400()
        {
            // Arrange
            var exception = new ArgumentException("Bad argument");
            RequestDelegate next = (HttpContext context) => throw exception;

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await _middleware.InvokeAsync(context, next);

            // Assert
            context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }
    }

    public class RateLimitingMiddlewareTests
    {
        [Fact]
        public async Task InvokeAsync_FirstRequest_ShouldAllow()
        {
            // Arrange
            var nextCalled = false;
            RequestDelegate next = (HttpContext context) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new RateLimitingMiddleware(next, requestsPerMinute: 10);
            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.1");
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            nextCalled.Should().BeTrue();
            context.Response.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task InvokeAsync_ExceedRateLimit_ShouldReturn429()
        {
            // Arrange
            RequestDelegate next = (HttpContext context) => Task.CompletedTask;
            var middleware = new RateLimitingMiddleware(next, requestsPerMinute: 3);

            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.1");
            context.Response.Body = new MemoryStream();

            // Act - Make requests up to limit
            for (int i = 0; i < 3; i++)
            {
                await middleware.InvokeAsync(context);
                context = new DefaultHttpContext();
                context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.1");
                context.Response.Body = new MemoryStream();
            }

            // Act - One more request should be rate limited
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be((int)HttpStatusCode.TooManyRequests);
        }

        [Fact]
        public async Task InvokeAsync_DifferentIPs_ShouldTrackSeparately()
        {
            // Arrange
            RequestDelegate next = (HttpContext context) => Task.CompletedTask;
            var middleware = new RateLimitingMiddleware(next, requestsPerMinute: 2);

            var context1 = new DefaultHttpContext();
            context1.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.1");
            context1.Response.Body = new MemoryStream();

            var context2 = new DefaultHttpContext();
            context2.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.2");
            context2.Response.Body = new MemoryStream();

            // Act - Make 2 requests from IP1
            await middleware.InvokeAsync(context1);
            context1 = new DefaultHttpContext();
            context1.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.1");
            context1.Response.Body = new MemoryStream();
            await middleware.InvokeAsync(context1);

            // IP2 should still be able to make requests
            await middleware.InvokeAsync(context2);

            // Assert
            context2.Response.StatusCode.Should().Be(200);
        }
    }
}
