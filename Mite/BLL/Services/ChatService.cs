using Mite.BLL.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using Mite.DAL.Entities;
using Mite.Models;
using NLog;
using Mite.BLL.IdentityManagers;
using System.Collections.Generic;
using Mite.Helpers;
using Microsoft.AspNet.Identity;

namespace Mite.BLL.Services
{
    public interface IChatService : IDataService
    {
        Task<DataServiceResult> CreateAsync(ChatModel model);
        Task<ChatModel> GetAsync(Guid id, string companionName);
    }
    public class ChatService : DataService, IChatService
    {
        private readonly ChatRepository repo;
        private readonly ILogger logger;
        private readonly AppUserManager userManager;

        public ChatService(IUnitOfWork database, ILogger logger, AppUserManager userManager) : base(database)
        {
            repo = database.GetRepo<ChatRepository, Chat>();
            this.logger = logger;
            this.userManager = userManager;
        }

        public async Task<DataServiceResult> CreateAsync(ChatModel model)
        {
            var chat = new Chat
            {
                Members = model.Members.Select(x => new User
                {
                    Id = x
                }).ToList()
            };
            try
            {
                await repo.AddAsync(chat);
                return Success;
            }
            catch(Exception e)
            {
                logger.Error($"Ошибка при создании чата: {e.Message}");
                return CommonError;
            }
        }

        public async Task<ChatModel> GetAsync(Guid id, string companionName)
        {
            var chat = await repo.GetAsync(id);
            return new ChatModel
            {
                Id = chat.Id,
                Emojies = EmojiHelper.GetEmojies(),
                Name = "Диалог с " + companionName
            };
        }
    }
}