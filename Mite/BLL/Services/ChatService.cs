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
using Mite.CodeData.Constants;
using Mite.DAL.Filters;
using System.Web.Hosting;
using Mite.DAL.DTO;
using Mite.CodeData.Enums;

namespace Mite.BLL.Services
{
    public interface IChatService : IDataService
    {
        Task<DataServiceResult> CreateAsync(ChatModel model);
        Task<ChatModel> GetByMembersAsync(IEnumerable<string> userIds);
        Task<ChatModel> GetAsync(Guid id, string companionName);
        Task<DataServiceResult> RemoveAsync(Guid id, string userId);
        Task<DataServiceResult> UpdateAsync(ChatModel model);
        /// <summary>
        /// Получить чаты по пользователю
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <param name="writingChat">Если пользователь кому то пишет, надо добавить этот чат</param>
        /// <returns></returns>
        Task<List<ShortChatModel>> GetByUserAsync(string userId, Guid? writingChat);
        Task<List<PublicChatModel>> GetPublishedAsync(string userId, int page, string input);
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
            chat.Id = Guid.NewGuid();
            chat.Members = model.Members.Select(x => new ChatMember
            {
                UserId = x.Id,
                ChatId = chat.Id
            }).ToList();
            try
            {
                await repo.AddAsync(chat);
                model.Id = chat.Id;
                if (model.ImageSrc == null)
                    model.ImageSrc = PathConstants.AvatarSrc;
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

        public async Task<List<ShortChatModel>> GetByUserAsync(string userId, Guid? writingChatId)
        {
            var chats = (await repo.GetByUserAsync(userId)).ToList();
            if (writingChatId != null && !chats.Any(x => writingChatId == x.Id))
            {
                //Поскольку чат, не находящийся в списке либо удален, либо не имеет сообщений
                var writingChat = await repo.GetWithMembersAsync(writingChatId.Value);
                if(writingChat != null)
                {
                    var companion = (await userManager.FindByIdAsync(writingChat.Members.First(x => x.UserId != userId).UserId));
                    var wrChatDto = new UserChatDTO
                    {
                        NewMessagesCount = 0,
                        Name = companion.UserName,
                        ImageSrc = companion.AvatarSrc,
                        ImageSrcCompressed = companion.AvatarSrc,
                        CreatorId = writingChat.CreatorId,
                        Status = ChatMemberStatuses.InChat,
                        Type = ChatTypes.Private
                    };
                    chats.Insert(0, wrChatDto);
                }
            }

            return Mapper.Map<List<ShortChatModel>>(chats);
        }

        public async Task<List<PublicChatModel>> GetPublishedAsync(string userId, int page, string input)
        {
            const int range = 30;
            var offset = range * page;
            var filter = new PublicChatsFilter
            {
                UserId = userId,
                Range = range,
                Offset = offset,
                Input = input
            };
            var chats = await repo.GetPublishedAsync(filter);
            return Mapper.Map<List<PublicChatModel>>(chats);
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
                    FilesHelper.DeleteFile(imgSrc);
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
            if (model.ImageSrc != null && !string.Equals(model.ImageSrc, chat.ImageSrc) && !string.Equals(model.ImageSrc, chat.ImageSrcCompressed))
            {
                chat.ImageSrc = ImagesHelper.UpdateImage(chat.ImageSrc, model.ImageSrc);
                FilesHelper.DeleteFile(chat.ImageSrc);
                chat.ImageSrcCompressed = FilesHelper.ToVirtualPath(ImagesHelper.Resize(HostingEnvironment.MapPath(chat.ImageSrc), 100));
            }
            chat.Name = model.Name;
            chat.Type = model.ChatType;
            chat.MaxMembersCount = model.MaxMembersCount ?? chat.MaxMembersCount;
            try
            {
                await repo.UpdateAsync(chat);
                return Success;
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при удалении чата", e);
            }
        }
    }
}