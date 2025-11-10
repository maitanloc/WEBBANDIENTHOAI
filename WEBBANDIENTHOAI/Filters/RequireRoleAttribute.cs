
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
namespace WEBBANDIENTHOAI.Filters
{
    // usage: [RequireRole("Admin")] or [RequireRole("Admin,Staff")]
    public class RequireRoleAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string[] _roles;
        public RequireRoleAttribute(string roles)
        {
            _roles = roles?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? Array.Empty<string>();
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var http = context.HttpContext;
            var roleName = http.Session.GetString("RoleName");
            if (string.IsNullOrEmpty(roleName) || !_roles.Contains(roleName, StringComparer.OrdinalIgnoreCase))
            {
                // not authorized
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            await next();
        }
    }
}
