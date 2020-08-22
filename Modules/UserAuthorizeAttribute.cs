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
                var roles = Role.Split(",");
                var i = 0;

                foreach (var role in roles)
                {
                    if (context.HttpContext.User?.IsInRole(role) == true)
                    {
                        i = i + 1;
                    }
                }

                if (i == 0)
                {
                    context.Result = new RedirectResult("~/Account/AccessDenied");
                }
            }
        }
    }
}
