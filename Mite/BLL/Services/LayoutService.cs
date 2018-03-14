using AutoMapper;
using Microsoft.AspNet.Identity;
using Mite.BLL.Core;
using Mite.BLL.IdentityManagers;
using Mite.DAL.Infrastructure;
using Mite.Models;
using NLog;
using System;

namespace Mite.BLL.Services
{
    public interface ILayoutService : IDataService
    {
        LayoutModel GetModel(string userId);
    }
    public class LayoutService : DataService, ILayoutService
    {
        private readonly AppUserManager userManager;
        private readonly IDealService dealService;
        private readonly IChatMessagesService messagesService;
        private readonly IUserReviewService reviewService;

        public LayoutService(IUnitOfWork database, ILogger logger, AppUserManager userManager, IDealService dealService,
            IChatMessagesService messagesService, IUserReviewService reviewService) : base(database, logger)
        {
            this.userManager = userManager;
            this.dealService = dealService;
            this.messagesService = messagesService;
            this.reviewService = reviewService;
        }

        public LayoutModel GetModel(string userId)
        {
            var user = userManager.FindById(userId);
            var timeLeft = DateTime.UtcNow - user.RegisterDate;
            return new LayoutModel
            {
                EmailConfirmed = user.EmailConfirmed,
                HasExternalLogins = user.Logins.Count > 0,
                NewMessagesCount = messagesService.GetNewCount(userId),
                NewDealsCount = dealService.GetNewCount(userId),
                ReviewLeft = reviewService.IsReviewLeft(userId),
                RegisterDayLeft = timeLeft.TotalDays > 1,
                User = Mapper.Map<UserShortModel>(user)
            };
        }
    }
}