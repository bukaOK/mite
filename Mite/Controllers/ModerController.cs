using AutoMapper;
using Mite.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Mite.Controllers
{
    [Authorize(Roles = "moder")]
    public class ModerController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public ModerController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public ViewResult Index()
        {
            return View();
        }
        [HttpPost]
        public JsonResult Tags()
        {
            var tags = _unitOfWork.TagsRepository.GetAll();
            return JsonResponse(JsonResponseStatuses.Success, new
            {
                Confirmed = tags.Where(x => x.IsConfirmed),
                NotConfirmed = tags.Where(x => !x.IsConfirmed)
            });
        }
        [HttpPost]
        public Task BindTag(Guid fromId, Guid toId)
        {
            return _unitOfWork.TagsRepository.BindAsync(fromId, toId);
        }
        [HttpPost]
        public async Task UpdatePostTags(IList<string> tagsNames, Guid postId)
        {
            var tags = Mapper.Map<List<Tag>>(tagsNames);
            await _unitOfWork.TagsRepository.AddWithPostAsync(tags, postId);
        }
    }
}