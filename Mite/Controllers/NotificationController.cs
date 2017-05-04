using Microsoft.AspNet.Identity;
using Mite.BLL.IdentityManagers;
using Mite.BLL.Services;
using Mite.Core;
using Mite.Models;
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
        public Task<List<NotificationModel>> GetByUser(bool onlyNew)
        {
            return _notificationService.GetByUserAsync(User.Identity.GetUserId(), onlyNew);
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
            await _notificationService.AddAsync(model);
            return Ok();
        }
        [HttpPut]
        public Task Read()
        {
            return _notificationService.ReadAsync(User.Identity.GetUserId());
        }
        [HttpDelete]
        public async Task<IHttpActionResult> Clean()
        {
            await _notificationService.Clean(User.Identity.GetUserId());
            return Ok();
        }
    }
}
