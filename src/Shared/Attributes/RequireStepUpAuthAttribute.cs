using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ItSupportServer.src.Shared.Attributes
{
    /// <summary>
    /// Enforces step-up authentication for sensitive endpoints.
    /// When UnifiedSessionMiddleware detects suspicious activity (score ≥ 50),
    /// it sets context.Items["RequireStepUp"] = true. This attribute reads that
    /// signal and blocks the request with 403 + STEP_UP_REQUIRED error code.
    ///
    /// Usage: [RequireStepUpAuth] on any controller action that handles sensitive operations
    /// (e.g., password change, account deletion, bulk operations).
    ///
    /// Reference: OWASP Step-Up Authentication
    /// https://cheatsheetseries.owasp.org/cheatsheets/Session_Management_Cheat_Sheet.html
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireStepUpAuthAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Items.ContainsKey("RequireStepUp") &&
                context.HttpContext.Items["RequireStepUp"] is true)
            {
                context.Result = new ObjectResult(new
                {
                    error = "Step-up authentication required due to suspicious session activity",
                    errorCode = "STEP_UP_REQUIRED"
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }
    }
}
