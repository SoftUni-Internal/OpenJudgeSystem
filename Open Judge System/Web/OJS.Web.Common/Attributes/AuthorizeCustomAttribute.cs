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
            string redirectUrl = request.IsSecureConnection
                ? $"{request.UrlReferrer.Scheme}://{request.UrlReferrer.Authority}/Account/Login"
                : "/Account/Login"; // Default behavior (HTTP)

            redirectUrl += "?returnUrl=" + Uri.EscapeDataString(request.RawUrl);

            filterContext.Result = new RedirectResult(redirectUrl);
        }
    }
}