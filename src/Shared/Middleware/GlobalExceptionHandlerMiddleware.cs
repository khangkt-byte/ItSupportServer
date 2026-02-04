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
    /// References:
    /// - OWASP Error Handling Cheat Sheet
    /// - CWE-209: Information Exposure Through Error Messages
    /// - Microsoft Security Development Lifecycle (SDL)
    /// - Google API Design Guide
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

            httpContext.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
            httpContext.Response.ContentType = "application/problem+json; charset=utf-8";

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
                    "Validation failed | TraceId: {TraceId} | Path: {Path} | Errors: {Errors}",
                    context.TraceIdentifier,
                    context.Request.Path,
                    errorSummary);
            }
            else if (exception is UnauthorizedException || exception is ForbiddenException)
            {
                // ✅ Security logging (potential attack)
                _logger.LogWarning(
                    "Access denied | User: {UserId} | IP: {IP} | Path: {Path} | Method: {Method} | TraceId: {TraceId}",
                    context.User.FindFirst("sub")?.Value ?? "anonymous",
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    context.Request.Path,
                    context.Request.Method,
                    context.TraceIdentifier);
            }
            else
            {
                var logLevel = statusCode >= 500 ? LogLevel.Error : LogLevel.Warning;
                
                _logger.Log(logLevel, exception,
                    "Error occurred: {ErrorType} | StatusCode: {StatusCode} | TraceId: {TraceId} | Path: {Path}",
                    exception.GetType().Name,
                    statusCode,
                    context.TraceIdentifier,
                    context.Request.Path);
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
                    source = exception.Source
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
            _ => StatusCodes.Status500InternalServerError
        };

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
                    break;

                case OtpRequiredException otpEx:
                    // ✅ IMPROVED: Don't expose raw accountId
                    problemDetails.Extensions["requiresOtp"] = true;
                    // ✅ Use session token instead (implement GenerateOtpSessionToken)
                    // problemDetails.Extensions["sessionToken"] = GenerateOtpSessionToken(otpEx.AccountId);
                    
                    // ⚠️ If you must include accountId, hash it:
                    // problemDetails.Extensions["accountReference"] = HashAccountId(otpEx.AccountId);
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

            if (exception is TooManyAttemptsException)
            {
                return "Bạn đã thử quá nhiều lần. Vui lòng đợi một lúc rồi thử lại.";
            }

            if (exception is OtpRequiredException)
            {
                return "Yêu cầu xác thực hai yếu tố. Vui lòng nhập mã OTP.";
            }

            // ✅ IMPROVED: External service errors with categories
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