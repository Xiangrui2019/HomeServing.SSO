using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeServing.SSO.Modules
{
    public class UserAuthorizeAttribute : ActionFilterAttribute
    {
        public string Role { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.User?.Identity.IsAuthenticated == false)
            {
                context.Result = new RedirectResult("~/Account/Login");
                return;
            }

            if (Role != "")
            {
                if (context.HttpContext.User?.IsInRole(Role) == false)
                {
                    context.Result = new RedirectResult("~/Account/AccessDenied");
                    return;
                }
            }
        }
    }
}
