using Microsoft.AspNet.Identity;
using Mite.BLL.Services;
using Mite.Core;
using Mite.Helpers;
using Mite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mite.Controllers
{
    public class ChatsController : BaseController
    {
        private readonly IChatService chatService;

        public ChatsController(IChatService chatService)
        {
            this.chatService = chatService;
        }
        //public PartialViewResult DealChat(Guid id, string companion)
        //{
        //    var chat = chatService.GetByUsers(User.Identity.GetUserId(), userId);
        //    return PartialView("_DealChat", chat);
        //}
    }
}