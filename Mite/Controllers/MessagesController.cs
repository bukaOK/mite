using Microsoft.AspNet.Identity;
using Mite.BLL.Services;
using Mite.Core;
using Mite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Mite.Controllers
{
    public class MessagesController : BaseController
    {
        private readonly IChatMessagesService messagesService;

        public MessagesController(IChatMessagesService messagesService)
        {
            this.messagesService = messagesService;
        }
        [HttpPost]
        public async Task<ActionResult> Add(ChatMessageModel model)
        {
            model.Sender = new UserShortModel
            {
                Id = User.Identity.GetUserId()
            };
            var result = await messagesService.AddAsync(model);
            if (result.Succeeded)
            {
                var message = (ChatMessageModel)result.ResultData;
                return Json(JsonStatuses.Success, message);
            }
            return InternalServerError();
        }
    }
}