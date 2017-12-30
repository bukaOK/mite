using Mite.BLL.Core;
using System;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using Mite.DAL.Entities;
using Mite.Models;
using NLog;
using Mite.BLL.IdentityManagers;
using System.Collections.Generic;
using Mite.Helpers;
using AutoMapper;
using Mite.BLL.Helpers;
using System.Linq;

namespace Mite.BLL.Services
{
    public interface IChatService : IDataService
    {
        Task<DataServiceResult> CreateAsync(ChatModel model);
        Task<ChatModel> GetByMembersAsync(IEnumerable<string> userIds);
        Task<ChatModel> GetAsync(Guid id, string companionName);
        Task<DataServiceResult> RemoveAsync(Guid id, string userId);
        Task<DataServiceResult> UpdateAsync(ChatModel model);
        Task<IEnumerable<ShortChatModel>> GetByUserAsync(string userId);
        /// <summary>
        /// Подписчики, которые не состоят в чате(один на один) с пользователем
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<IEnumerable<UserShortModel>> GetActualFollowersAsync(string userId);
    }
    public class ChatService : DataService, IChatService
    {
        private readonly ChatRepository repo;
        private readonly AppUserManager userManager;
        private readonly IFollowersService followersService;
        private readonly IChatMessagesService messagesService;

        public ChatService(IUnitOfWork database, ILogger logger, AppUserManager userManager, IFollowersService followersService, 
            IChatMessagesService messagesService) : base(database, logger)
        {
            repo = database.GetRepo<ChatRepository, Chat>();
            this.userManager = userManager;
            this.followersService = followersService;
            this.messagesService = messagesService;
        }

        public async Task<DataServiceResult> CreateAsync(ChatModel model)
        {
            var chat = Mapper.Map<Chat>(model);
            try
            {
                await repo.AddAsync(chat);
                model.Id = chat.Id;
                return DataServiceResult.Success(model);
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при создании чата", e);
            }
        }

        public async Task<IEnumerable<UserShortModel>> GetActualFollowersAsync(string userId)
        {
            var followers = await repo.GetActualFollowersAsync(userId);
            return Mapper.Map<IEnumerable<UserShortModel>>(followers);
        }

        public async Task<ChatModel> GetAsync(Guid id, string companionName)
        {
            var chat = await repo.GetAsync(id);
            return new ChatModel
            {
                Id = chat.Id,
                Emojies = EmojiHelper.GetEmojies(),
                Name = "Чат с " + companionName
            };
        }

        public async Task<ChatModel> GetByMembersAsync(IEnumerable<string> userIds)
        {
            var chat = await repo.GetByMembersAsync(userIds);
            return chat == null ? null : Mapper.Map<ChatModel>(chat);
        }

        public async Task<IEnumerable<ShortChatModel>> GetByUserAsync(string userId)
        {
            var chats = await repo.GetByUserAsync(userId);
            var newMessagesCount = await Database.GetRepo<ChatMessagesRepository, ChatMessage>()
                .GetNewCountAsync(userId, chats.Select(x => x.Id).ToList());

            return Mapper.Map<IEnumerable<ShortChatModel>>(chats, opts =>
            {
                opts.Items.Add("messagesCount", newMessagesCount);
            });
        }

        public async Task<DataServiceResult> RemoveAsync(Guid id, string userId)
        {
            try
            {
                var isMember = await repo.IsMemberAsync(id, userId);
                if (!isMember)
                    return DataServiceResult.Failed("Неизвестный пользователь");
                var msgs = await Database.GetRepo<ChatMessagesRepository, ChatMessage>().GetByChatAsync(id);
                await messagesService.RemoveListAsync(msgs.ToList(), userId);
                var (imgSrc, compressedSrc) = await repo.RemoveAsync(id, userId);
                if (!string.IsNullOrEmpty(imgSrc))
                {
                    ImagesHelper.DeleteImage(imgSrc, compressedSrc);
                }
                return Success;
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при удалении чата", e);
            }
        }

        public async Task<DataServiceResult> UpdateAsync(ChatModel model)
        {
            var chat = await repo.GetAsync(model.Id);
            if (!string.Equals(model.ImageSrc, chat.ImageSrc) && !string.Equals(model.ImageSrc, chat.ImageSrcCompressed))
            {
                var (vPath, compressedVPath) = ImagesHelper.UpdateImage(chat.ImageSrc, chat.ImageSrcCompressed, model.ImageSrc);
                chat.ImageSrc = vPath;
                chat.ImageSrcCompressed = compressedVPath;
            }
            chat.Name = model.Name;
            chat.Type = model.ChatType;
            chat.MaxMembersCount = model.MaxMembersCount ?? chat.MaxMembersCount;
            try
            {
                await UpdateAsync(model);
                return Success;
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при удалении чата", e);
            }
        }
    }
}