using Microsoft.AspNet.Identity;
using Mite.BLL.IdentityManagers;
using Mite.BLL.Services;
using Mite.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace Mite.Controllers
{
    public class NotificationController : ApiController
    {
        private readonly INotificationService _notificationService;
        private readonly AppUserManager _userManager;

        public NotificationController(INotificationService notificationService, AppUserManager userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }
        [HttpGet]
        public Task<List<NotificationModel>> GetByUser()
        {
            return _notificationService.GetNotificationsByUser(User.Identity.GetUserId());
        }
        [HttpPost]
        public async Task<IHttpActionResult> AddNotification(NotificationModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            if (model.User.Id == User.Identity.GetUserId())
                return Ok();
            try
            {
                model.NotifyUser = new UserShortModel
                {
                    Id = User.Identity.GetUserId()
                };
                await _notificationService.AddNotification(model);
                return Ok();
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }
        [HttpPut]
        public Task ReadNotifications()
        {
            return _notificationService.ReadNotificationsAsync(User.Identity.GetUserId());
        }
        [HttpDelete]
        public Task<int> GetNotificationsCount()
        {
            return _notificationService.GetNewNotificationsCountAsync(User.Identity.GetUserId());
        }
    }
}
