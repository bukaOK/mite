using Autofac;
using Hangfire;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security.DataProtection;
using Mite.BLL.IdentityManagers;
using Mite.BLL.Services;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.ExternalServices.Google;
using NLog;
using Owin;
using System;
using System.Linq;
using System.Net.Http;

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

            GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnection");
            app.UseHangfireDashboard();
            app.UseHangfireServer();
            RecurringJob.AddOrUpdate(() => LoadAdSenseIncome(), Cron.Daily);
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
            IGoogleService googleService = new GoogleService(unitOfWork, container.Resolve<HttpClient>(), logger);
            ICashService cashService = new CashService(unitOfWork, logger);

            //От имени админа всегда отправляются запросы
            var admin = userManager.FindByName("landenor");

            //За какой день начислить доход
            var incomeDay = DateTime.UtcNow.AddDays(-1);
            var dailyIncomeTask = googleService.GetAdsenseSumAsync(incomeDay, incomeDay, admin.Id);
            dailyIncomeTask.Wait();
            var dailyIncome = dailyIncomeTask.Result;
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
    }
}