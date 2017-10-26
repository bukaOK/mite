using Microsoft.AspNet.Identity;
using Mite.BLL.Services;
using Mite.CodeData.Enums;
using Mite.Core;
using Mite.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Mite.Controllers.Api
{
    public class MessagesController : BaseController
    {
        private readonly IChatMessagesService messagesService;

        public MessagesController(IChatMessagesService messagesService)
        {
            this.messagesService = messagesService;
        }
    }
}