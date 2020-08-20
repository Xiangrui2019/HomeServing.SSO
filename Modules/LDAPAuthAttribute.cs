using System;
using System.Text;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HomeServing.SSO.Modules
{
    class LDAPAuthAttribute : Attribute, IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var token = context.HttpContext.Request.Headers["Authorization"];
            var rawstring = Encoding.Default.GetString(
                Convert.FromBase64String(token)
            );
            var splited = rawstring.Split("%");
            var username = splited[0];
            var password = splited[1];

            
        }
    }
}