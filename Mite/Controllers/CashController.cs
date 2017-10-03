﻿using Microsoft.AspNet.Identity;
using Mite.BLL.IdentityManagers;
using Mite.BLL.Services;
using Mite.CodeData.Constants;
using Mite.CodeData.Enums;
using Mite.Core;
using Mite.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Mite.Controllers
{
    [Authorize]
    public class CashController : BaseController
    {
        private readonly AppUserManager userManager;
        private readonly ICashService cashService;
        /// <summary>
        /// Общая комиссия за все платежи(вывод денег) 2%.
        /// </summary>
        private const double Comission = 0.02;

        public CashController(AppUserManager userManager, ICashService cashService)
        {
            this.userManager = userManager;
            this.cashService = cashService;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="epr">External payment result(результат ответа от банка)</param>
        /// <returns></returns>
        public async Task<ActionResult> Index()
        {
            var cash = await cashService.GetUserCashAsync(User.Identity.GetUserId());
            var model = new CashModel
            {
                CashSum = cash,
                IsYandexAuthorized = await cashService.IsYandexAuthorized(User.Identity.GetUserId()),
                ClientId = YaMoneySettings.ClientId,
                RedirectUri = YaMoneySettings.RedirectUri,
                SystemYandexWallet = YaMoneySettings.WalletId
            };
            return View(model);
        }
        public PartialViewResult Wallets()
        {
            var user = userManager.FindById(User.Identity.GetUserId());
            var model = new WalletsSettingsModel
            {
                YandexWalId = user.YandexWalId
            };
            return PartialView("Wallets", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateWallets(WalletsSettingsModel model)
        {
            var user = await userManager.FindByIdAsync(User.Identity.GetUserId());
            var isPasswordValid = await userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordValid)
                return Json(JsonStatuses.ValidationError, "Неверный пароль");

            user.YandexWalId = model.YandexWalId;
            await userManager.UpdateAsync(user);
            return Json(JsonStatuses.Success, "Яндекс кошелек успешно добавлен/обновлен");
        }
        [HttpPost]
        public async Task<JsonResult> PaymentsHistory()
        {
            var payments = await cashService.GetPaymentsHistoryAsync(User.Identity.GetUserId());
            return Json(JsonStatuses.Success, payments);
        }
        [HttpPost]
        public async Task<JsonResult> Referals()
        {
            var referals = await cashService.GetReferalsByUserAsync(User.Identity.GetUserId());
            return Json(JsonStatuses.Success, referals);
        }
        public PartialViewResult Advertising()
        {
            var user = userManager.FindById(User.Identity.GetUserId());
            var operations = cashService.GetByType(User.Identity.GetUserId(), CashOperationTypes.GoogleAd);
            
            return PartialView(new CashAdvertisingModel
            {
                AllowShowAd = user.ShowAd,
                Income = operations.Sum(x => x.Sum),
                DailyIncome = operations.
                    Where(x => DateTime.Now.AddDays(-2) <= x.Date &&
                    x.Date <= DateTime.Now).Sum(x => x.Sum),
                WeekIncome = operations.Where(x => DateTime.Now.AddDays(-7) <= x.Date && x.Date <= DateTime.Now).Sum(x => x.Sum),
                MonthIncome = operations.Where(x => DateTime.Now.AddMonths(-1) <= x.Date && x.Date <= DateTime.Now).Sum(x => x.Sum)
            });
        }
        [HttpPost]
        public async Task<JsonResult> AdConfirm(CashAdvertisingModel model)
        {
            var user = await userManager.FindByIdAsync(User.Identity.GetUserId());
            user.ShowAd = model.AllowShowAd;
            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Json(JsonStatuses.Success);
            }
            return Json(JsonStatuses.Error);
        }
    }
}