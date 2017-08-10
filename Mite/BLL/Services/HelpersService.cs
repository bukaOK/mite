using Mite.BLL.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mite.DAL.Infrastructure;
using Mite.Models;
using System.Threading.Tasks;
using Mite.DAL.Entities;
using AutoMapper;
using Mite.Enums;
using Microsoft.AspNet.Identity;
using Mite.DAL.Repositories;

namespace Mite.BLL.Services
{
    public interface IHelpersService : IDataService
    {
        Task<IdentityResult> InitHelperAsync(HelperTypes helperType, string userId);
        Task<HelperModel> GetByUserAsync(string userId);
        HelperModel GetByUser(string userId);
    }
    public class HelpersService : DataService, IHelpersService
    {
        public HelpersService(IUnitOfWork database) : base(database)
        {
        }

        public HelperModel GetByUser(string userId)
        {
            var repo = Database.GetRepo<HelpersRepository, Helper>();
            var helper = repo.GetByUser(userId);
            if (helper == null)
            {
                helper = new Helper
                {
                    UserId = userId
                };
                repo.Add(helper);
            }
            return Mapper.Map<HelperModel>(helper);
        }

        public async Task<HelperModel> GetByUserAsync(string userId)
        {
            var repo = Database.GetRepo<HelpersRepository, Helper>();
            var helper = await repo.GetByUserAsync(userId);
            if (helper == null)
            {
                helper = new Helper
                {
                    UserId = userId
                };
                await repo.AddAsync(helper);
            }
            return Mapper.Map<HelperModel>(helper);
        }

        public async Task<IdentityResult> InitHelperAsync(HelperTypes helperType, string userId)
        {
            var repo = Database.GetRepo<HelpersRepository, Helper>();
            var helper = await repo.GetByUserAsync(userId);
            switch (helperType)
            {
                case HelperTypes.EditDocBtn:
                    helper.EditDocBtn = true;
                    break;
                case HelperTypes.PublicationsBtn:
                    helper.PublicPostsBtn = true;
                    break;
                default:
                    return IdentityResult.Failed("Неизвестный тип помощника");
            }
            await repo.UpdateAsync(helper);
            return IdentityResult.Success;
        }
    }
}