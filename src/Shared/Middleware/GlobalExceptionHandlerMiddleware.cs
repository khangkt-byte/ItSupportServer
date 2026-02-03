using ItSupportServer.src.Shared.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ItSupportServer.src.Shared.Middleware
{
    /// <summary>
    /// Global exception handler following RFC 7807 Problem Details standard
    /// Security: OWASP compliant, CWE-209 mitigation
    /// Pattern: Separate user-facing (Vietnamese) and developer (English) messages
    /// 
    /// Enhancements:
    /// - Error codes (machine-readable)
    /// - Error enrichment (metadata, categories)
    /// - Retry-After header (RFC 7231)
    /// - Batch error handling (207 Multi-Status)
    /// 
    /// References:
    /// - RFC 7807: Problem Details for HTTP APIs
    /// - RFC 7231: Retry-After Header
    /// - OWASP Error Handling Cheat Sheet
    /// - CWE-209: Information Exposure Through Error Messages
    /// - Microsoft Security Development Lifecycle (SDL)
    /// - Google API Design Guide
    /// - Stripe API Error Handling
    /// - Auth0 Error Codes Catalog
    /// </summary>
    public class GlobalExceptionHandlerMiddleware : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IHostEnvironment _env;

        // ✅ Protected resource paths (prevent enumeration)
        private static readonly string[] ProtectedPaths = 
        {
            "/api/accounts",
            "/api/roles",
            "/api/employees"
        };

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

            // ✅ LOGS (English - for developers & security team)
            LogException(httpContext, exception, problemDetails.Status ?? 500);

            // ✅ Set HTTP status code
            httpContext.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
            httpContext.Response.ContentType = "application/problem+json; charset=utf-8";

            // ✅ ENHANCEMENT 1: Add Retry-After header for 429
            if (exception is TooManyAttemptsException rateLimitEx && rateLimitEx.RetryAfterSeconds.HasValue)
            {
                httpContext.Response.Headers["Retry-After"] = rateLimitEx.RetryAfterSeconds.Value.ToString();
            }

            // ✅ ENHANCEMENT 2: Add custom headers for error tracking
            httpContext.Response.Headers["X-Error-Code"] = GetErrorCode(exception);
            httpContext.Response.Headers["X-Correlation-ID"] = httpContext.TraceIdentifier;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        private void LogException(HttpContext context, Exception exception, int statusCode)
        {
            if (exception is ValidationException validationEx)
            {
                var errorSummary = string.Join("; ", 
                    validationEx.Errors.SelectMany(kvp => 
                        kvp.Value.Select(msg => $"{kvp.Key}: {msg}")));
                
                _logger.LogWarning(
                    "Validation failed | " +
                    "TraceId: {TraceId} | " +
                    "Path: {Path} | " +
                    "Method: {Method} | " +  // ✅ ADD
                    "User: {UserId} | " +  // ✅ ADD
                    "Errors: {Errors} | " +
                    "ErrorCount: {ErrorCount}",  // ✅ ADD
                    context.TraceIdentifier,
                    context.Request.Path,
                    context.Request.Method,
                    context.User.FindFirst("sub")?.Value ?? "anonymous",
                    errorSummary,
                    validationEx.Errors.Sum(e => e.Value.Length));
            }
            else if (exception is UnauthorizedException || exception is ForbiddenException)
            {
                // ✅ Security logging (potential attack)
                _logger.LogWarning(
                    "Access denied | " +
                    "User: {UserId} | " +
                    "IP: {IP} | " +  // ✅ Consider sanitizing in production
                    "Path: {Path} | " +
                    "Method: {Method} | " +
                    "UserAgent: {UserAgent} | " +  // ✅ ADD
                    "TraceId: {TraceId} | " +
                    "ExceptionType: {ExceptionType}",  // ✅ ADD
                    context.User.FindFirst("sub")?.Value ?? "anonymous",
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    context.Request.Path,
                    context.Request.Method,
                    context.Request.Headers["User-Agent"].ToString(),
                    context.TraceIdentifier,
                    exception.GetType().Name);
            }
            else if (exception is BatchOperationException batchEx)
            {
                // ✅ ENHANCEMENT: Batch operation logging
                _logger.LogWarning(
                    "Batch operation completed | " +
                    "Total: {Total} | " +
                    "Success: {Success} | " +
                    "Failures: {Failures} | " +
                    "SuccessRate: {SuccessRate:P} | " +  // ✅ ADD: Percentage
                    "TraceId: {TraceId} | " +
                    "Path: {Path} | " +
                    "User: {UserId}",  // ✅ ADD
                    batchEx.Results.Count,
                    batchEx.Results.Count(r => r.Success),
                    batchEx.Results.Count(r => !r.Success),
                    (double)batchEx.Results.Count(r => r.Success) / batchEx.Results.Count,
                    context.TraceIdentifier,
                    context.Request.Path,
                    context.User.FindFirst("sub")?.Value ?? "anonymous");
            }
            else if (exception is NotFoundException notFoundEx)
            {
                // ✅ ADD: Log not found (helps identify broken links)
                _logger.LogInformation(
                    "Resource not found | " +
                    "Path: {Path} | " +
                    "Method: {Method} | " +
                    "Resource: {Resource} | " +
                    "TraceId: {TraceId}",
                    context.Request.Path,
                    context.Request.Method,
                    notFoundEx.Message,
                    context.TraceIdentifier);
            }
            else
            {
                var logLevel = statusCode >= 500 ? LogLevel.Error : LogLevel.Warning;
                
                _logger.Log(logLevel, exception,
                    "Error occurred | " +
                    "ErrorType: {ErrorType} | " +
                    "StatusCode: {StatusCode} | " +
                    "TraceId: {TraceId} | " +
                    "Path: {Path} | " +
                    "Method: {Method} | " +  // ✅ ADD
                    "QueryString: {QueryString} | " +  // ✅ ADD
                    "User: {UserId} | " +  // ✅ ADD
                    "IP: {IP} | " +  // ✅ ADD
                    "ErrorCode: {ErrorCode} | " +
                    "Category: {Category}",  // ✅ ADD
                    exception.GetType().Name,
                    statusCode,
                    context.TraceIdentifier,
                    context.Request.Path,
                    context.Request.Method,
                    context.Request.QueryString.ToString(),
                    context.User.FindFirst("sub")?.Value ?? "anonymous",
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    GetErrorCode(exception),
                    GetErrorCategory(exception));
            }
        }

        private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
        {
            var statusCode = GetStatusCode(exception);

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = GetUserFriendlyTitle(exception),
                Detail = GetUserFriendlyDetail(exception, statusCode, context),
                Instance = context.Request.Path,
                Type = $"https://httpstatuses.com/{statusCode}"
            };

            // ✅ Add metadata (camelCase for JSON)
            problemDetails.Extensions["traceId"] = context.TraceIdentifier;
            problemDetails.Extensions["timestamp"] = DateTime.UtcNow;
            
            // ✅ ENHANCEMENT 1: Add machine-readable error code
            problemDetails.Extensions["errorCode"] = GetErrorCode(exception);
            
            // ✅ ENHANCEMENT 2: Add error category
            problemDetails.Extensions["errorCategory"] = GetErrorCategory(exception);

            // ✅ Add exception-specific extensions
            AddExceptionSpecificData(problemDetails, exception);

            // 🔧 Development-only debug info (ENGLISH - for developers)
            if (_env.IsDevelopment())
            {
                problemDetails.Extensions["debug"] = new
                {
                    exceptionType = exception.GetType().Name,
                    stackTrace = exception.StackTrace,
                    innerException = exception.InnerException?.Message,
                    source = exception.Source,
                    metadata = (exception as AppException)?.Metadata 
                };
            }

            return problemDetails;
        }

        private static int GetStatusCode(Exception exception) => exception switch
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
            BatchOperationException => StatusCodes.Status207MultiStatus,  // ✅ ADD
            _ => StatusCodes.Status500InternalServerError
        };

        /// <summary>
        /// Get machine-readable error code
        /// Pattern: Stripe API, Microsoft Graph
        /// </summary>
        private static string GetErrorCode(Exception exception)
        {
            return exception switch
            {
                AppException appEx => appEx.Code,
                ArgumentNullException => "NULL_ARGUMENT",
                ArgumentException => "INVALID_ARGUMENT",
                InvalidOperationException => "INVALID_OPERATION",
                TimeoutException => "TIMEOUT",
                _ => "INTERNAL_ERROR"
            };
        }

        /// <summary>
        /// Get error category for classification
        /// Pattern: Google API Design Guide
        /// </summary>
        private static string GetErrorCategory(Exception exception)
        {
            return exception switch
            {
                ValidationException => "VALIDATION",
                UnauthorizedException or ForbiddenException or OtpRequiredException => "AUTHENTICATION",
                NotFoundException => "RESOURCE",
                ConflictException => "CONFLICT",
                BusinessRuleException => "BUSINESS_LOGIC",
                TooManyAttemptsException => "RATE_LIMIT",
                ExternalServiceException => "EXTERNAL_SERVICE",
                BatchOperationException => "BATCH_OPERATION",
                _ when exception is AppException => "APPLICATION",
                _ => "SYSTEM"
            };
        }

        private void AddExceptionSpecificData(ProblemDetails problemDetails, Exception exception)
        {
            switch (exception)
            {
                case ValidationException validationEx:
                    // ✅ SAFE: Validation errors CAN be detailed
                    problemDetails.Extensions["errors"] = validationEx.Errors;
                    problemDetails.Extensions["errorCount"] = validationEx.Errors.Sum(e => e.Value.Length);
                    break;

                case BusinessRuleException businessEx:
                    // ✅ SAFE: Business rule code (sanitized)
                    problemDetails.Extensions["errorCode"] = businessEx.Code;
                    
                    // ✅ ENHANCEMENT: Add metadata if available
                    if (businessEx.Metadata != null && businessEx.Metadata.Any())
                    {
                        problemDetails.Extensions["metadata"] = businessEx.Metadata;
                    }
                    break;

                case OtpRequiredException otpEx:
                    // ✅ Client needs to know OTP is required
                    problemDetails.Extensions["requiresOtp"] = true;
                    // ✅ Don't expose raw accountId (security)
                    break;

                case TooManyAttemptsException rateLimitEx:
                    // ✅ ENHANCEMENT: Add retry information
                    if (rateLimitEx.RetryAfterSeconds.HasValue)
                    {
                        problemDetails.Extensions["retryAfter"] = rateLimitEx.RetryAfterSeconds.Value;
                        problemDetails.Extensions["retryAfterHuman"] = FormatRetryAfter(rateLimitEx.RetryAfterSeconds.Value);
                    }
                    break;

                case BatchOperationException batchEx:
                    // ✅ ENHANCEMENT: Batch error details
                    problemDetails.Extensions["batchResults"] = batchEx.Results.Select(r => new
                    {
                        id = r.ItemId,
                        success = r.Success,
                        statusCode = r.StatusCode,
                        error = r.Success ? null : new
                        {
                            code = r.ErrorCode,
                            message = r.ErrorMessage
                        }
                    });
                    
                    problemDetails.Extensions["summary"] = new
                    {
                        total = batchEx.Results.Count,
                        succeeded = batchEx.Results.Count(r => r.Success),
                        failed = batchEx.Results.Count(r => !r.Success)
                    };
                    break;

                case NotFoundException notFoundEx:
                    // ✅ ENHANCEMENT: Add resource metadata
                    if (notFoundEx.Metadata != null)
                    {
                        problemDetails.Extensions["resource"] = notFoundEx.Metadata;
                    }
                    break;

                case ConflictException conflictEx:
                    // ✅ ENHANCEMENT: Add conflict metadata
                    if (conflictEx.Metadata != null)
                    {
                        problemDetails.Extensions["conflictInfo"] = conflictEx.Metadata;
                    }
                    break;

                case ExternalServiceException extEx:
                    // ✅ ENHANCEMENT: Add service info (generic)
                    problemDetails.Extensions["serviceType"] = "external";
                    problemDetails.Extensions["retryable"] = true;
                    break;
            }
        }

        private string GetUserFriendlyDetail(Exception exception, int statusCode, HttpContext context)
        {
            // ✅ SAFE: Validation, business rules, conflicts - VIETNAMESE
            if (exception is ValidationException || 
                exception is BusinessRuleException ||
                exception is ConflictException)
            {
                return exception.Message; // Already sanitized in custom exceptions
            }

            // ✅ Batch operations (special handling)
            if (exception is BatchOperationException batchEx)
            {
                var failCount = batchEx.Results.Count(r => !r.Success);
                return $"Hoàn thành với {failCount} lỗi. Xem chi tiết trong 'batchResults'.";
            }

            // ✅ IMPROVED: NotFound with enumeration protection
            if (exception is NotFoundException)
            {
                // ✅ OWASP recommendation: Generic message for protected resources
                if (IsProtectedResource(context) && !IsAuthenticated(context))
                {
                    return "Không tìm thấy tài nguyên yêu cầu.";
                }
                
                // For public resources or authenticated users, detailed message is OK
                return exception.Message;
            }

            // ⚠️ Auth errors - Generic (prevent enumeration)
            if (exception is UnauthorizedException)
            {
                return "Phiên đăng nhập không hợp lệ hoặc đã hết hạn. Vui lòng đăng nhập lại.";
            }

            if (exception is ForbiddenException)
            {
                return "Bạn không có quyền truy cập tài nguyên này. Vui lòng liên hệ quản trị viên.";
            }

            if (exception is TooManyAttemptsException rateLimitEx)
            {
                // ✅ ENHANCEMENT: Include retry info in message
                if (rateLimitEx.RetryAfterSeconds.HasValue)
                {
                    var retryTime = FormatRetryAfter(rateLimitEx.RetryAfterSeconds.Value);
                    return $"Bạn đã thử quá nhiều lần. Vui lòng đợi {retryTime} rồi thử lại.";
                }
                return "Bạn đã thử quá nhiều lần. Vui lòng đợi một lúc rồi thử lại.";
            }

            if (exception is OtpRequiredException)
            {
                return "Yêu cầu xác thực hai yếu tố. Vui lòng nhập mã OTP.";
            }

            // ✅ External service errors
            if (exception is ExternalServiceException extEx)
            {
                return extEx.Message; // Should be generic like "Dịch vụ tạm thời không khả dụng"
            }

            // 🚨 Server errors - NEVER expose details
            if (statusCode >= 500)
            {
                return $"Đã xảy ra lỗi hệ thống. Vui lòng liên hệ bộ phận hỗ trợ với mã lỗi: {context.TraceIdentifier}";
            }

            // ✅ Default fallback
            return _env.IsDevelopment() 
                ? exception.Message 
                : GetGenericUserMessage(statusCode);
        }

        private static string GetUserFriendlyTitle(Exception exception) => exception switch
        {
            NotFoundException => "Không tìm thấy",
            ValidationException => "Dữ liệu không hợp lệ",
            UnauthorizedException => "Chưa đăng nhập",
            ForbiddenException => "Không có quyền truy cập",
            ConflictException => "Dữ liệu trùng lặp",
            BusinessRuleException => "Vi phạm quy tắc nghiệp vụ",
            TooManyAttemptsException => "Quá nhiều yêu cầu",
            ExternalServiceException => "Lỗi dịch vụ bên ngoài",
            OtpRequiredException => "Yêu cầu xác thực OTP",
            BatchOperationException => "Kết quả xử lý hàng loạt",
            _ => "Lỗi hệ thống"
        };

        private static string GetGenericUserMessage(int statusCode) => statusCode switch
        {
            404 => "Không tìm thấy tài nguyên yêu cầu.",
            400 => "Dữ liệu gửi lên không đúng định dạng.",
            401 => "Vui lòng đăng nhập để tiếp tục.",
            403 => "Bạn không có quyền thực hiện thao tác này.",
            409 => "Dữ liệu bị trùng lặp.",
            422 => "Yêu cầu không thể xử lý do vi phạm quy tắc nghiệp vụ.",
            429 => "Bạn đã gửi quá nhiều yêu cầu. Vui lòng chờ một chút.",
            502 => "Không thể kết nối đến dịch vụ. Vui lòng thử lại sau.",
            503 => "Dịch vụ tạm thời không khả dụng. Vui lòng thử lại sau.",
            _ => "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại sau hoặc liên hệ bộ phận hỗ trợ."
        };

        /// <summary>
        /// Format retry-after seconds to human-readable format
        /// </summary>
        private static string FormatRetryAfter(int seconds)
        {
            if (seconds < 60)
                return $"{seconds} giây";
            
            var minutes = seconds / 60;
            if (minutes < 60)
                return $"{minutes} phút";
            
            var hours = minutes / 60;
            return $"{hours} giờ";
        }

        // ✅ Helper methods for security checks
        private static bool IsProtectedResource(HttpContext context)
        {
            return ProtectedPaths.Any(path => 
                context.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsAuthenticated(HttpContext context)
        {
            return context.User.Identity?.IsAuthenticated == true;
        }
    }
}