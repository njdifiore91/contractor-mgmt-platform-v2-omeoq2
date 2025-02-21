// Package versions:
// Microsoft.AspNetCore.Http 6.0.0
// Microsoft.Extensions.Logging 6.0.0
// Microsoft.AspNetCore.Hosting 6.0.0
// System.Text.Json 6.0.0

using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Backend.API.Middleware
{
    /// <summary>
    /// Advanced middleware component that provides comprehensive exception handling with 
    /// environment-aware responses, structured logging, and security features.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of the ExceptionHandlingMiddleware.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline</param>
        /// <param name="logger">Logger for structured logging</param>
        /// <param name="environment">Web host environment for conditional error details</param>
        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }

        /// <summary>
        /// Executes the middleware with enhanced error handling and logging.
        /// </summary>
        /// <param name="context">The HTTP context for the current request</param>
        public async Task InvokeAsync(HttpContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            try
            {
                // Generate and set correlation ID
                var correlationId = Guid.NewGuid().ToString();
                context.Response.Headers.Add("X-Correlation-ID", correlationId);

                // Execute next middleware
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Creates secure, environment-aware error responses.
        /// </summary>
        /// <param name="context">The HTTP context for the current request</param>
        /// <param name="exception">The exception that was caught</param>
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var correlationId = context.Response.Headers["X-Correlation-ID"].ToString();
            
            // Determine status code based on exception type
            var statusCode = exception switch
            {
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                ArgumentException => HttpStatusCode.BadRequest,
                KeyNotFoundException => HttpStatusCode.NotFound,
                _ => HttpStatusCode.InternalServerError
            };

            // Create error response object
            var errorResponse = new
            {
                CorrelationId = correlationId,
                Message = GetSanitizedErrorMessage(exception, statusCode),
                DetailedError = _environment.IsDevelopment() ? exception.ToString() : null,
                StatusCode = (int)statusCode
            };

            // Log the error with structured logging
            _logger.LogError(
                exception,
                "Error processing request. CorrelationId: {CorrelationId}, Path: {Path}, StatusCode: {StatusCode}",
                correlationId,
                context.Request.Path,
                (int)statusCode);

            // Set response details
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            // Serialize and write response
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(errorResponse, _jsonOptions));
        }

        /// <summary>
        /// Returns a sanitized error message based on the environment and status code.
        /// </summary>
        private string GetSanitizedErrorMessage(Exception exception, HttpStatusCode statusCode)
        {
            if (_environment.IsDevelopment())
            {
                return exception.Message;
            }

            // Production-safe error messages
            return statusCode switch
            {
                HttpStatusCode.Unauthorized => "Unauthorized access",
                HttpStatusCode.NotFound => "Resource not found",
                HttpStatusCode.BadRequest => "Invalid request",
                _ => "An unexpected error occurred"
            };
        }
    }
}