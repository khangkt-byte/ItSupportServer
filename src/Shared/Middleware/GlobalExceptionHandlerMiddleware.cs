using ItSupportServer.src.Shared.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ItSupportServer.src.Shared.Middleware
{
    /// <summary>
    /// Global exception handler following RFC 7807 Problem Details standard
    /// </summary>
    public class GlobalExceptionHandlerMiddleware : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandlerMiddleware(
            ILogger<GlobalExceptionHandlerMiddleware> logger,
            IHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var problemDetails = CreateProblemDetails(httpContext, exception);

            // Log with structured data
            _logger.LogError(
                exception,
                "Error occurred: {ErrorType} | TraceId: {TraceId} | Path: {Path}",
                exception.GetType().Name,
                httpContext.TraceIdentifier,
                httpContext.Request.Path);

            httpContext.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
            httpContext.Response.ContentType = "application/problem+json";

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
        {
            var statusCode = exception switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                ValidationException => StatusCodes.Status400BadRequest,
                UnauthorizedException => StatusCodes.Status401Unauthorized,
                ForbiddenException => StatusCodes.Status403Forbidden,
                ConflictException => StatusCodes.Status409Conflict,
                BusinessRuleException => StatusCodes.Status422UnprocessableEntity,
                TooManyAttemptsException => StatusCodes.Status429TooManyRequests,
                ExternalServiceException => StatusCodes.Status502BadGateway,
                OtpRequiredException => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = GetTitle(exception),
                Detail = GetDetail(exception),
                Instance = context.Request.Path,
                Type = $"https://httpstatuses.com/{statusCode}"
            };

            // Add trace ID for correlation
            problemDetails.Extensions["traceId"] = context.TraceIdentifier;
            problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

            // Add validation errors if applicable
            if (exception is ValidationException validationEx)
            {
                problemDetails.Extensions["errors"] = validationEx.Errors;
            }

            // Only expose detailed errors in Development
            if (!_env.IsDevelopment())
            {
                problemDetails.Detail = GetSafeMessage(statusCode);
            }
            else
            {
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
                problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
            }

            // Handle OtpRequiredException specially
            if (exception is OtpRequiredException otpEx)
            {
                problemDetails.Extensions["accountId"] = otpEx.AccountId;
                problemDetails.Extensions["requiresOtp"] = true;
            }

            return problemDetails;
        }

        private static string GetTitle(Exception exception) => exception switch
        {
            NotFoundException => "Resource Not Found",
            ValidationException => "Validation Failed",
            UnauthorizedException => "Unauthorized Access",
            ForbiddenException => "Forbidden",
            ConflictException => "Conflict",
            _ => "An error occurred"
        };

        private static string GetDetail(Exception exception) => exception switch
        {
            Exceptions.ApplicationException appEx => appEx.Message,
            _ => exception.Message
        };

        private static string GetSafeMessage(int statusCode) => statusCode switch
        {
            404 => "The requested resource was not found",
            400 => "The request was invalid",
            401 => "Authentication is required",
            403 => "You don't have permission to access this resource",
            409 => "The request conflicts with existing data",
            _ => "An unexpected error occurred. Please try again later."
        };
    }
}