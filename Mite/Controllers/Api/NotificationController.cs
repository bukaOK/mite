using Microsoft.AspNet.Identity;
using Mite.BLL.IdentityManagers;
using Mite.BLL.Services;
using Mite.Core;
using Mite.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Mite.Controllers
{
    [Authorize]
    public class NotificationController : ApiController
    {
        private readonly INotificationService notificationService;
        private readonly ILogger logger;

        public NotificationController(INotificationService notificationService, ILogger logger)
        {
            this.notificationService = notificationService;
            this.logger = logger;
        }
        [HttpGet]
        public async Task<List<NotificationModel>> GetByUser(bool onlyNew, CancellationToken cancellationToken)
        {
            var result = await notificationService.GetByUserAsync(User.Identity.GetUserId(), onlyNew);
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }
            return result;
        }
        [HttpPost]
        public async Task<IHttpActionResult> Add(NotificationModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            if (model.User.Id == User.Identity.GetUserId())
                return Ok();
            model.NotifyUser = new UserShortModel
            {
                Id = User.Identity.GetUserId()
            };
            await notificationService.AddAsync(model);
            return Ok();
        }
        [HttpPut]
        public Task Read()
        {
            return notificationService.ReadAsync(User.Identity.GetUserId());
        }
        [HttpDelete]
        public async Task<IHttpActionResult> Clean()
        {
            try
            {
                await notificationService.Clean(User.Identity.GetUserId());
                return Ok();
            }
            catch(Exception e)
            {
                logger.Error($"Ошибка при очистке уведомлений: {e.Message}");
                return InternalServerError();
            }
        }
    }
}
