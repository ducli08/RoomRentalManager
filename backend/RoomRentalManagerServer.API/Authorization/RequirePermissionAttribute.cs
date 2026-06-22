using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RoomRentalManagerServer.Application.Interfaces;

namespace RoomRentalManagerServer.API.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : Attribute, IAsyncActionFilter
    {
        public string? Permission { get; set; }
        public string[] AnyOf { get; set; } = Array.Empty<string>();

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var checker = context.HttpContext.RequestServices.GetRequiredService<IPermissionChecker>();

            bool allowed;
            if (!string.IsNullOrEmpty(Permission))
            {
                allowed = await checker.HasPermissionAsync(Permission);
            }
            else if (AnyOf.Length > 0)
            {
                allowed = await checker.HasAnyPermissionAsync(AnyOf);
            }
            else
            {
                allowed = false;
            }

            if (!allowed)
            {
                context.Result = new ObjectResult(new { message = "Forbidden: insufficient permissions." })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            await next();
        }
    }
}
