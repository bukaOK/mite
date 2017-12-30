using Microsoft.AspNet.Identity;
using Mite.BLL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;

namespace Mite.Controllers.Api
{
    [Authorize]
    public class MessageController : ApiController
    {
        private readonly IChatMessagesService messagesService;

        public MessageController(IChatMessagesService messagesService)
        {
            this.messagesService = messagesService;
        }
        public async Task<IHttpActionResult> Get(Guid chatId, int page)
        {
            const int range = 20;
            try
            {
                var messages = await messagesService.GetByFilterAsync(chatId, User.Identity.GetUserId(), page, range);
                return Json(new
                {
                    data = messages,
                    ended = messages.Count() < range
                });
            }
            catch(ArgumentException e)
            {
                return BadRequest(e.Message);
            }
        }
        //[HttpPost]
        //public async Task<IHttpActionResult> Add(ChatMessageModel model)
        //{
        //    model.Sender = new UserShortModel
        //    {
        //        Id = User.Identity.GetUserId()
        //    };
        //    return InternalServerError();
        //    var result = await messagesService.AddAsync(model);
        //    if (result.Succeeded)
        //    {
        //        var message = (ChatMessageModel)result.ResultData;
        //        return Ok(message);
        //    }
        //    return Content(HttpStatusCode.ServiceUnavailable, result.Errors);
        //}
        [HttpDelete]
        public async Task<IHttpActionResult> Remove([FromUri]List<Guid> ids)
        {
            var result = await messagesService.RemoveListAsync(ids, User.Identity.GetUserId());
            if (result.Succeeded)
                return Ok();
            return Content(HttpStatusCode.ServiceUnavailable, result.Errors);
        }
    }
}
