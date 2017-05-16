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

namespace Mite.BLL.Services
{
    public interface IHelpersService
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
            var helper = Database.HelpersRepository.GetByUser(userId);
            if (helper == null)
            {
                helper = new Helper
                {
                    Id = Guid.NewGuid(),
                    UserId = userId
                };
                Database.HelpersRepository.Add(helper);
                helper = Database.HelpersRepository.Get(helper.Id);
            }
            return Mapper.Map<HelperModel>(helper);
        }

        public async Task<HelperModel> GetByUserAsync(string userId)
        {
            var helper = await Database.HelpersRepository.GetByUserAsync(userId);
            if (helper == null)
            {
                helper = new Helper
                {
                    Id = Guid.NewGuid(),
                    UserId = userId
                };
                await Database.HelpersRepository.AddAsync(helper);
                helper = await Database.HelpersRepository.GetAsync(helper.Id);
            }
            return Mapper.Map<HelperModel>(helper);
        }

        public async Task<IdentityResult> InitHelperAsync(HelperTypes helperType, string userId)
        {
            var helper = await Database.HelpersRepository.GetByUserAsync(userId);
            switch (helperType)
            {
                case HelperTypes.EditDocBtn:
                    helper.EditDocBtn = true;
                    break;
                default:
                    return IdentityResult.Failed("Неизвестный тип помощника");
            }
            await Database.HelpersRepository.UpdateAsync(helper);
            return IdentityResult.Success;
        }
    }
}