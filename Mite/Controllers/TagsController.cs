﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;
using AutoMapper;
using System.Web.Http;
using Mite.Models;
using Mite.DAL.Entities;
using Mite.BLL.DTO;
using Mite.DAL.Repositories;

namespace Mite.Controllers
{
    [Authorize(Roles = "moder")]
    public class TagsController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public TagsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IEnumerable<TagModel>> GetForUser()
        {
            var tags = await _unitOfWork.GetRepo<TagsRepository, Tag>().GetAllWithPopularityAsync(true);
            return Mapper.Map<IEnumerable<TagModel>>(tags);
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IEnumerable<string>> GetByName(string name)
        {
            var tags = await _unitOfWork.GetRepo<TagsRepository, Tag>().GetByNameAsync(name);
            return Mapper.Map<IEnumerable<string>>(tags);
        }
        [HttpPost]
        public async Task<Tag> Add(Tag tag)
        {
            tag.Id = Guid.NewGuid();
            tag.IsConfirmed = true;
            tag.Checked = true;
            await _unitOfWork.GetRepo<TagsRepository, Tag>().AddAsync(tag);
            return tag;
        }
        [HttpPut]
        public Task Update(Tag tag)
        {
            return _unitOfWork.GetRepo<TagsRepository, Tag>().UpdateAsync(tag);
        }
        [HttpDelete]
        public Task Delete(Guid id)
        {
            return _unitOfWork.GetRepo<TagsRepository, Tag>().RemoveAsync(id);
        }
    }
}