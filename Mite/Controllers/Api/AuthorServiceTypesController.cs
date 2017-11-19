using Mite.BLL.Services;
using Mite.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Mite.Models;
using Mite.CodeData.Constants;

namespace Mite.Controllers.Api
{
    [Authorize(Roles = RoleNames.Moderator)]
    public class AuthorServiceTypesController : ApiController
    {
        private readonly IAuthorServiceTypeService serviceTypeService;

        public AuthorServiceTypesController(IAuthorServiceTypeService serviceTypeService)
        {
            this.serviceTypeService = serviceTypeService;
        }
        [HttpGet]
        public async Task<IEnumerable<AuthorServiceType>> GetAll()
        {
            var serviceTypes = await serviceTypeService.GetAllAsync();
            return serviceTypes;
        }
        [HttpPost]
        [Authorize]
        public async Task<IHttpActionResult> Add(ServiceTypeModel model)
        {
            if (string.IsNullOrEmpty(model.Name))
                return BadRequest(ModelState);

            if (User.IsInRole(RoleNames.Moderator))
                model.Confirmed = true;
            else
                model.Confirmed = false;

            var result = await serviceTypeService.AddAsync(model);
            if (result.Succeeded)
                return Json(model);
            return InternalServerError();
        }
        [HttpPut]
        public async Task<IHttpActionResult> Update(ServiceTypeModel model)
        {
            var result = await serviceTypeService.UpdateAsync(model);
            if (result.Succeeded)
                return Json(model);
            return InternalServerError();
        }
        [HttpDelete]
        public async Task<IHttpActionResult> Remove(Guid id)
        {
            var result = await serviceTypeService.RemoveAsync(id);
            if (result.Succeeded)
                return Ok();
            return InternalServerError();
        }
    }
}
