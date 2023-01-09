using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace eBookStore.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string? _roles;
        public AuthorizeAttribute()
        {

        }

        public AuthorizeAttribute(string roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            string token = context.HttpContext.Session.GetString("jwtToken");
            string role = context.HttpContext.Session.GetString("role");

            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(role))
            {
                context.Result = new RedirectToActionResult("Index", "Unauthorized", null);
                return;
            }

            if (!string.IsNullOrEmpty(_roles) && !_roles.Contains(role))
            {
                context.Result = new RedirectToActionResult("Index", "Unauthorized", null);
                return;
            }
        }
    }
}