namespace OJS.Web.Infrastructure.SimpleInjectorPackages
{
    using System.Collections.Generic;
    using Microsoft.AspNet.Identity.EntityFramework;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Services.Common.HttpRequester;
    using OJS.Web.Common;
    using OJS.Web.Infrastructure.Seeders;
    using SimpleInjector;
    using SimpleInjector.Packaging;

    public class SeederPackage : IPackage
    {
        public void RegisterServices(Container container)
        {
            container.Register<IEnumerable<ISeeder>>(
                () =>
                {
                    var seeders = new List<ISeeder>();
                    var userManager = new OjsUserManager<UserProfile>(new UserStore<UserProfile>(container.GetInstance<OjsDbContext>()));
                    seeders.Add(new AdministratorSeeder(userManager, container.GetInstance<IHttpRequesterService>()));

                    return seeders;
                }, Lifestyle.Transient);
        }
    }
}