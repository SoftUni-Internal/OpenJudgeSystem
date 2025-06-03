namespace OJS.Web.Common.Attributes
{
    using System;
    using System.Web.Mvc;

    public class AuthorizeCustomAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var request = filterContext.HttpContext.Request;

            // Generate the redirect URL with HTTPS if needed.
            // When apps work on http, but Load Balancer calls them over HTTPS,
            // we want to redirect back to https.
            var forwardedProto = request.Headers["X-Forwarded-Proto"];
            var scheme = !string.IsNullOrEmpty(forwardedProto)
                ? forwardedProto
                : (request.IsSecureConnection ? "https" : "http");

            var authority = request.Url.Authority;
            var loginUrl = $"{scheme}://{authority}/Account/Login";
            var returnUrl = request.RawUrl ?? "/";

            var redirectUrl = $"{loginUrl}?returnUrl={Uri.EscapeDataString(returnUrl)}";

            filterContext.Result = new RedirectResult(redirectUrl);
        }
    }
}