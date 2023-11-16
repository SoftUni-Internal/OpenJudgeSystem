namespace OJS.Servers.Ui.Infrastructure.Extensions
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Hosting;
    using OJS.Servers.Infrastructure.Extensions;

    public static class WebApplicationExtensions
    {
        public static WebApplication ConfigureWebApplication(
            this WebApplication app,
            string apiVersion)
        {
            app
                .UseDefaults()
                .MapDefaultRoutes()
                .UseReactStaticFiles();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwaggerDocs(apiVersion.ToApiName());
            }

            //Added here, because if it is added in the end immediately after the 200 response (Healthy) the FE redirects to 404 page.
            app.UseHealthMonitoring();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    "default",
                    "{controller=Home}/{action=Index}");
                endpoints.MapFallbackToController("index", "home");
            });

            if (app.Environment.IsDevelopment())
            {
                app.UseSpaStaticFiles();
                app.UseSpa(spa =>
                {
                    spa.Options.SourcePath = "ClientApp";
                    if (app.Environment.IsDevelopment())
                    {
                        // spa.UseProxyToSpaDevelopmentServer("htp://");
                        spa.UseReactDevelopmentServer(npmScript: "start");
                    }
                });
            }

            return app;
        }

        public static WebApplication UseReactStaticFiles(this WebApplication app)
        {
            // var distDirectory = app.Environment.IsDevelopment()
            //     ? "dist"
            //     : "build";
            var distDirectory = "build";
            var reactFilesPath = Path.Combine(Directory.GetCurrentDirectory(), "ClientApp", distDirectory);
            if (!Directory.Exists(reactFilesPath))
            {
                Directory.CreateDirectory(reactFilesPath);
            }

            app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(reactFilesPath), });
            return app;
        }
    }
}