using Microsoft.AspNet.Identity;
using Mite.BLL.Services;
using Mite.CodeData.Constants;
using Mite.Models;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Mite.Controllers.Api
{
    [Authorize(Roles = RoleNames.Author)]
    public class AuthorServicesController : ApiController
    {
        private readonly IAuthorServiceService authorServiceService;

        public AuthorServicesController(IAuthorServiceService authorServiceService)
        {
            this.authorServiceService = authorServiceService;
        }
        [HttpPost]
        public async Task<IHttpActionResult> Add(AuthorServiceModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            model.AuthorId = User.Identity.GetUserId();
            var result = await authorServiceService.AddAsync(model);
            if (result.Succeeded)
            {
                return Ok();
            }
            return Content(HttpStatusCode.ServiceUnavailable, result.Errors);
        }
        [HttpPut]
        public async Task<IHttpActionResult> Update(AuthorServiceModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await authorServiceService.UpdateAsync(model);
            if (result.Succeeded)
            {
                return Ok();
            }
            return Content(HttpStatusCode.ServiceUnavailable, result.Errors);
        }
        [HttpDelete]
        public async Task<IHttpActionResult> Remove(Guid id)
        {
            var result = await authorServiceService.RemoveAsync(id, User.Identity.GetUserId());
            if (result.Succeeded)
            {
                return Ok();
            }
            return Content(HttpStatusCode.ServiceUnavailable, result.Errors);
        }
    }
}
