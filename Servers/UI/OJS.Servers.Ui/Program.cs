namespace OJS.Servers.Ui
{
    using Microsoft.AspNetCore.Builder;
    using OJS.Servers.Infrastructure.Extensions;
    using OJS.Servers.Ui.Extensions;

    internal class Program
    {
        private const string ApiVersion = "v1";

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.ConfigureServices(builder.Configuration, ApiVersion);
            builder.Host.UseLogger(builder.Environment);

            var app = builder.Build();

            app.ConfigureWebApplication(builder.Configuration, ApiVersion);
            app.Run();
        }
    }
}