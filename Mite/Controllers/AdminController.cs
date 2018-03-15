﻿using AutoMapper;
using Microsoft.AspNet.Identity;
using Mite.Attributes.Filters;
using Mite.BLL.IdentityManagers;
using Mite.CodeData.Constants;
using Mite.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using Mite.ExternalServices.YandexMoney;
using Mite.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Mite.Controllers
{
    [Authorize(Roles = RoleNames.Admin)]
    [AjaxOnly("Index")]
    public class AdminController : BaseController
    {
        private readonly AppUserManager userManager;
        private readonly IYandexMoneyService yandexService;
        private readonly AppRoleManager roleManager;
        private readonly IUnitOfWork unitOfWork;

        public AdminController(AppUserManager userManager, IYandexMoneyService yandexService, AppRoleManager roleManager, 
            IUnitOfWork unitOfWork)
        {
            this.userManager = userManager;
            this.yandexService = yandexService;
            this.roleManager = roleManager;
            this.unitOfWork = unitOfWork;
        }
        public ActionResult Index()
        {
            return View();
        }
        public PartialViewResult ModerRegister()
        {
            var role = roleManager.FindByName("moder");
            var users = userManager.Users.Where(x => x.Roles.Any(y => y.RoleId == role.Id));
            return PartialView(users);
        }
        [HttpPost]
        public async Task<ActionResult> AddModer(string userName)
        {
            if(string.IsNullOrEmpty(userName))
                return Json(JsonStatuses.ValidationError, "Заполни поле с именем модератора");
            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
                return Json(JsonStatuses.ValidationError, "Не найден пользователь с таким именем.");
            await userManager.AddToRoleAsync(user.Id, "moder");
            return Json(JsonStatuses.Success, null);
        }
        public PartialViewResult Statistic()
        {
            var model = new AdminStatisticModel()
            {
                UsersCount = userManager.Users.Count()
            };
            var repo = unitOfWork.GetRepo<PostsRepository, Post>();
            model.PostsCount = repo.GetCount();
            return PartialView(model);
        }
        public ActionResult Reviews()
        {
            var reviews = unitOfWork.GetRepo<ReviewsRepository, UserReview>().GetAll();
            var model = Mapper.Map<IEnumerable<AdminUserReviewModel>>(reviews);
            return PartialView(model);
        }
    }
}