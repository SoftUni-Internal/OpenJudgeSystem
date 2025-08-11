namespace OJS.Web.Common.Attributes
{
    using System;
    using System.Web.Mvc;

    public class AuthorizeCustomAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var request = filterContext.HttpContext.Request;

            var returnUrl = request.RawUrl ?? "/";
            if (!new UrlHelper(filterContext.RequestContext).IsLocalUrl(returnUrl))
            {
                returnUrl = "/";
            }

            var redirectUrl = $"/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl)}";
            filterContext.Result = new RedirectResult(redirectUrl);
        }
    }
}