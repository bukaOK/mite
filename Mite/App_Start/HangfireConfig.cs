using Autofac;
using Hangfire;
using Hangfire.Annotations;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Security.DataProtection;
using Mite.BLL.IdentityManagers;
using Mite.BLL.Services;
using Mite.CodeData.Constants;
using Mite.CodeData.Enums;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.ExternalServices.Google;
using NLog;
using Owin;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web.Hosting;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Mite
{
    public static class HangfireConfig
    {
        private static IContainer container;
        private static IAppBuilder owinApp;

        public static void Initialize(IAppBuilder app, IContainer diContainer)
        {
            container = diContainer;
            owinApp = app;
            GlobalConfiguration.Configuration.UsePostgreSqlStorage("DefaultConnection");
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new [] { new HangfireAuthFilter() }
            });
            app.UseHangfireServer();
            RecurringJob.AddOrUpdate(() => LoadAdSenseIncome(), Cron.Daily);
            RecurringJob.AddOrUpdate(() => ClearImagesCache(), Cron.Weekly);
            RecurringJob.AddOrUpdate(() => TariffsCheckoutAsync(), Cron.Minutely);
        }
        /// <summary>
        /// Раздаем заработок за день пользователям
        /// </summary>
        public static void LoadAdSenseIncome()
        {
            var logger = container.Resolve<ILogger>();
            var dbContext = new AppDbContext();
            var userManager = new AppUserManager(new UserStore<User>(dbContext), owinApp.GetDataProtectionProvider());
            IUnitOfWork unitOfWork = new UnitOfWork();
            IGoogleAdSenseService googleService = new GoogleAdSenseService(unitOfWork, container.Resolve<HttpClient>(), logger);
            ICashService cashService = new CashService(unitOfWork, logger);

            //От имени админа всегда отправляются запросы
            var admin = userManager.FindByName("landenor");

            //За какой день начислить доход
            var incomeDay = DateTime.UtcNow.AddDays(-1);
            var dailyIncome = googleService.GetAdsenseSumAsync(incomeDay, incomeDay, admin.Id).GetAwaiter().GetResult();
            
            if (dailyIncome == 0)
                return;
            //30% остается у MiteGroup - остальное пользователям
            var authorsIncome = dailyIncome * 0.7;
            //Пользователи, разрешившие показывать рекламу
            var showAdAuthors = cashService.GetAdUsers();
            var parameterSum = showAdAuthors.Sum(x => x.Parameter);

            foreach (var author in showAdAuthors)
            {
                if (author.Parameter == 0)
                    continue;
                if (parameterSum == 0)
                    break;
                //Часть от общего дохода(0 < incomePart <= 1)
                var incomePart = (double)author.Parameter / parameterSum;
                if (incomePart <= 0 || incomePart == double.NaN)
                    continue;
                cashService.AdSensePay(author.Id, incomePart * authorsIncome, incomeDay);
            }
        }

        public static void ClearImagesCache()
        {
            var cacheDir = HostingEnvironment.MapPath(PathConstants.VirtualImageCacheFolder);
            var cacheLifetime = new TimeSpan(3, 0, 0, 0);

            foreach(var dir in Directory.EnumerateDirectories(cacheDir))
            {
                var info = new DirectoryInfo(dir);
                if (DateTime.UtcNow - info.LastAccessTimeUtc < cacheLifetime)
                    info.Delete(true);
            }
        }

        public static async Task TariffsCheckoutAsync()
        {
            using(var scope = container.BeginLifetimeScope())
            {
                var dbContext = scope.Resolve<AppDbContext>();
                //var unitOfWork = scope.Resolve<IUnitOfWork>();
                var cashService = scope.Resolve<ICashService>();

                var tariffs = await dbContext.ClientTariffs.Include(x => x.Tariff).Where(x => x.PayStatus == TariffStatuses.Paid).ToListAsync();
                foreach (var clTariff in tariffs)
                {
                    if((DateTime.UtcNow - clTariff.LastPayTimeUtc).Days >= 30)
                    {
                        var cash = await cashService.GetUserCashAsync(clTariff.ClientId);
                        if(cash - clTariff.Tariff.Price < 0)
                        {
                            clTariff.PayStatus = TariffStatuses.NotPaid;
                        }
                        else
                        {
                            await cashService.AddAsync(clTariff.ClientId, clTariff.Tariff.AuthorId, clTariff.Tariff.Price, CashOperationTypes.TariffPay);
                            clTariff.LastPayTimeUtc = DateTime.UtcNow;
                            clTariff.PayStatus = TariffStatuses.Paid;
                            dbContext.Entry(clTariff).State = EntityState.Modified;
                        }
                        dbContext.Entry(clTariff).State = EntityState.Modified;
                    }
                }
                await dbContext.SaveChangesAsync();
            }
        }
    }
    public class HangfireAuthFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            var owinContext = new OwinContext(context.GetOwinEnvironment());
            return owinContext.Authentication.User.IsInRole(RoleNames.Admin);
        }
    }
}