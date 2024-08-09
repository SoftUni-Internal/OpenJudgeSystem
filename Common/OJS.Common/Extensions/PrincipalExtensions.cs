namespace OJS.Common.Extensions
{
    using System.Security.Claims;
    using System.Security.Principal;

    public static class PrincipalExtensions
    {
        public static string? GetId(this ClaimsPrincipal? principal)
            => principal?.FindFirstValue(ClaimTypes.NameIdentifier);

        public static string? GetUsername(this ClaimsPrincipal? principal)
            => principal?.FindFirstValue(ClaimTypes.Name);

        public static bool IsAuthenticated(this IPrincipal principal)
            => principal.Identity?.IsAuthenticated ?? false;

        public static bool IsAdmin(this IPrincipal principal)
            => principal.IsInRole(GlobalConstants.Roles.Administrator);

        public static bool IsLecturer(this IPrincipal principal)
            => principal.IsInRole(GlobalConstants.Roles.Lecturer);

        public static bool IsDeveloper(this IPrincipal principal)
            => principal.IsInRole(GlobalConstants.Roles.Developer);

        public static bool IsAdminOrLecturer(this IPrincipal principal)
            => principal.IsAdmin() || principal.IsLecturer();
    }
}