namespace OJS.Web
{
    using OJS.Web.Infrastructure.Seeders;
    using Owin;

    // NOSONAR
    public partial class Startup
    {
        public void RunSeeders(IAppBuilder app)
        {
            var seeders = SimpleInjectorConfig.Container.GetAllInstances<ISeeder>();

            foreach (var seeder in seeders)
            {
                seeder.SeedData();
            }
        }
    }
}