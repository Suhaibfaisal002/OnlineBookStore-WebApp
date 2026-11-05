using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Online_Booking_Store.Infrastructure;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SessionAuthorizeAttribute : ActionFilterAttribute
{
    private readonly HashSet<string> _allowedRoles;

    public SessionAuthorizeAttribute(params string[] allowedRoles)
    {
        _allowedRoles = allowedRoles.Length == 0
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(allowedRoles, StringComparer.OrdinalIgnoreCase);
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var httpContext = context.HttpContext;
        var userId = httpContext.Session.GetInt32(SessionKeys.UserId);
        var role = httpContext.Session.GetString(SessionKeys.UserRole);

        if (!userId.HasValue || string.IsNullOrWhiteSpace(role))
        {
            var returnUrl = $"{httpContext.Request.Path}{httpContext.Request.QueryString}";
            context.Result = new RedirectToActionResult("Login", "Customer", new { returnUrl });
            return;
        }

        if (_allowedRoles.Count > 0 && !_allowedRoles.Contains(role))
        {
            context.Result = new RedirectToActionResult("Index", "Home", null);
        }
    }
}
