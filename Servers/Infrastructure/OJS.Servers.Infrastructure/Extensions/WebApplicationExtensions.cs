namespace OJS.Servers.Infrastructure.Extensions
{
    using Hangfire;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Hosting;
    using OJS.Servers.Infrastructure.Filters;
    using OJS.Servers.Infrastructure.Middleware;
    using SoftUni.AutoMapper.Infrastructure.Extensions;
    using static OJS.Common.GlobalConstants.Urls;

    public static class WebApplicationExtensions
    {
        public static WebApplication UseDefaults(this WebApplication app)
        {
            app.UseCustomExceptionHandling();

            app.UseAutoMapper();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            return app;
        }

        public static WebApplication MapDefaultRoutes(this WebApplication app)
        {
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            return app;
        }

        public static WebApplication UseAndMapHangfireDashboard(this WebApplication app)
        {
            var dashboardOptions = new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() },
            };

            app.UseHangfireDashboard(HangfirePath);
            app.MapHangfireDashboard(dashboardOptions);

            return app;
        }

        public static WebApplication UseSwaggerDocs(this WebApplication app, string name)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint($"/swagger/{name}/swagger.json", name);
            });

            return app;
        }

        public static WebApplication UseCustomExceptionHandling(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler(errorApp => errorApp.Run(new DevelopmentExceptionMiddleware().Get));
            }
            else
            {
                app.UseExceptionHandler(errorApp => errorApp.Run(new Rfc7807ExceptionMiddleware().Get));
            }

            return app;
        }
    }
}