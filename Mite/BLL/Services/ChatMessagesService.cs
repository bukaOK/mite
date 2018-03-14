using Mite.BLL.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Mite.DAL.Infrastructure;
using System.Threading.Tasks;
using Mite.Models;
using Mite.DAL.Repositories;
using Mite.DAL.Entities;
using Mite.Helpers;
using Mite.BLL.IdentityManagers;
using NLog;
using AutoMapper;
using Mite.BLL.Helpers;
using Mite.CodeData.Enums;
using System.Web.Hosting;

namespace Mite.BLL.Services
{
    public interface IChatMessagesService : IDataService
    {
        Task<IEnumerable<ChatMessageModel>> GetByFilterAsync(Guid chatId, string userId, int page, int range);
        /// <summary>
        /// Добавление сообщения
        /// </summary>
        /// <param name="model">Модель</param>
        /// <param name="inSession">Находится ли чат в сессии(недавно созданный)</param>
        /// <returns></returns>
        Task<DataServiceResult> AddAsync(ChatMessageModel model, string currentUserId, bool inSession);
        Task<DataServiceResult> RemoveListAsync(List<Guid> ids, string userId);
        Task<DataServiceResult> ReadAsync(Guid id, string userId);
        int GetNewCount(string userId);
    }
    public class ChatMessagesService : DataService, IChatMessagesService
    {
        private const string SecKey = "AOgdKZhNixsujiLUWhqmeCYTqu7FWB5Q";
        private const string AttachmentsFolder = "~/Public/attachments";

        private readonly ChatMessagesRepository messagesRepository;
        private readonly ChatRepository chatRepository;
        private readonly AppUserManager userManager;

        public ChatMessagesService(IUnitOfWork database, AppUserManager userManager, ILogger logger) : base(database, logger)
        {
            messagesRepository = database.GetRepo<ChatMessagesRepository, ChatMessage>();
            chatRepository = database.GetRepo<ChatRepository, Chat>();
            this.userManager = userManager;
        }

        public async Task<DataServiceResult> AddAsync(ChatMessageModel model, string currentUserId, bool inSession)
        {
            Chat chat = null;
            var blackListRepo = Database.GetRepo<BlackListUserRepository, BlackListUser>();
            //В сессии могут находиться только приватные чаты(диалоги)
            if (inSession)
            {
                var members = model.Recipients.Select(x => x.Id).Take(2);
                var companionId = members.First(x => x != currentUserId);
                var isInBlackList = (await blackListRepo.IsInBlackListAsync(currentUserId, companionId)) ||
                        (await blackListRepo.IsInBlackListAsync(companionId, currentUserId));
                if (isInBlackList)
                    return DataServiceResult.Failed("Пользователь в черном списке");
                chat = await chatRepository.GetByMembersAsync(members);
                if(chat == null)
                {
                    chat = new Chat
                    {
                        Id = model.ChatId,
                        Type = ChatTypes.Private,
                        Members = model.Recipients.Select(x => new ChatMember
                        {
                            EnterDate = DateTime.UtcNow,
                            Status = ChatMemberStatuses.InChat,
                            UserId = x.Id,
                            ChatId = model.ChatId
                        }).Take(2).ToList()
                    };
                    await chatRepository.AddAsync(chat);
                }
            }
            else
            {
                chat = await chatRepository.GetWithMembersAsync(model.ChatId);
                var currentMember = chat.Members.FirstOrDefault(x => x.UserId == currentUserId);
                if(currentMember == null)
                    return DataServiceResult.Failed("Неизвестный пользователь");
                if (currentMember.Status != ChatMemberStatuses.InChat)
                    return DataServiceResult.Failed("Вы не состоите в чате");
                if (chat.Type == ChatTypes.Private && chat.Members.Count == 2)
                {
                    var companionId = chat.Members.First(x => x.UserId != currentUserId).UserId;
                    var isInBlackList = (await blackListRepo.IsInBlackListAsync(currentUserId, companionId)) || 
                        (await blackListRepo.IsInBlackListAsync(companionId, currentUserId));
                    if (isInBlackList)
                        return DataServiceResult.Failed("Пользователь в черном списке");
                }
                if (chat == null)
                    return DataServiceResult.Failed("Неизвестный чат");
                    
                foreach(var member in chat.Members)
                {
                    if(member.Status == ChatMemberStatuses.Removed)
                    {
                        member.Status = ChatMemberStatuses.InChat;
                    }
                }
            }

            var key = CryptoHelper.CreateKey(chat.Id.ToString() + model.Sender.Id + SecKey);
            var encrypted = await CryptoHelper.EncryptAsync(model.Message, key);
            var message = new ChatMessage
            {
                ChatId = chat.Id,
                IV = encrypted.iv,
                Content = encrypted.data,
                SenderId = model.Sender.Id,
                SendDate = DateTime.UtcNow,
                Recipients = chat.Members.Where(x => x.Status == ChatMemberStatuses.InChat).Select(x =>
                {
                    var re = new ChatMessageUser
                    {
                        UserId = x.UserId
                    };
                    if (x.UserId == model.Sender.Id)
                        re.Read = true;

                    return re;
                }).ToList()
            };
            if(model.StreamAttachments != null && model.StreamAttachments.Count() > 0)
            {
                message.Attachments = model.StreamAttachments.Select(file =>
                {
                    var att = new ChatMessageAttachment
                    {
                        Type = FilesHelper.AttachmentTypeMime(file.ContentType),
                        MessageId = message.Id,
                        Name = file.FileName.Length <= 150 
                            ? file.FileName : $"{file.FileName.Substring(0, 146)}.{file.FileName.Split('.').Last()}"
                    };
                    switch (att.Type)
                    {
                        case AttachmentTypes.Image:
                            att.Src = FilesHelper.CreateImage(AttachmentsFolder, file);
                            att.CompressedSrc = FilesHelper.ToVirtualPath(ImagesHelper.Resize(HostingEnvironment.MapPath(att.Src), 500));
                            break;
                        default:
                            att.Src = FilesHelper.CreateFile(AttachmentsFolder, file);
                            break;
                    }
                    return att;
                }).ToList();
            }
            try
            {
                await messagesRepository.AddAsync(message);

                model.Id = message.Id;
                model.Sender = Mapper.Map<UserShortModel>(await userManager.FindByIdAsync(message.SenderId));
                model.Attachments = Mapper.Map<List<MessageAttachmentModel>>(message.Attachments);
                model.Recipients = message.Recipients.Select(x => new UserShortModel
                {
                    Id = x.UserId
                }).ToList();
                model.Chat = Mapper.Map<ShortChatModel>(chat);
                model.StreamAttachments = null;
                return DataServiceResult.Success(model);
            }
            catch(Exception e)
            {
                logger.Error($"Ошибка при создании сообщения {e.Message}");
                return DataServiceResult.Failed("Внутренняя ошибка");
            }
        }

        public async Task<IEnumerable<ChatMessageModel>> GetByFilterAsync(Guid chatId, string userId, int page, int range)
        {
            var chat = await chatRepository.GetWithMembersAsync(chatId);
            //Когда чат в сессии
            if (chat == null)
                return new List<ChatMessageModel>();
            if (chat.Type != ChatTypes.Public && !chat.Members.Any(x => x.UserId == userId))
                throw new ArgumentException("Неизвестный пользователь");
            var offset = (page - 1) * range;

            var messages = await messagesRepository.GetAsync(chatId, range, offset, userId);
            return Mapper.Map<IEnumerable<ChatMessageModel>>(messages, opts => opts.Items.Add("currentUserId", userId));
        }

        public int GetNewCount(string userId)
        {
            var count = messagesRepository.GetNewCount(userId);
            return count;
        }

        public async Task<DataServiceResult> ReadAsync(Guid id, string userId)
        {
            var message = await messagesRepository.GetAsync(id);
            if (message == null)
                return DataServiceResult.Failed("Сообщение не найдено");
            try
            {
                await messagesRepository.ReadAsync(id, userId);
                return DataServiceResult.Success();
            }
            catch(Exception e)
            {
                logger.Error($"Id: {message.Id}. Ошибка при чтении сообщения {e.Message}");
                return DataServiceResult.Failed("Внутренняя ошибка");
            }
        }

        public async Task<DataServiceResult> RemoveListAsync(List<Guid> ids, string userId)
        {
            try
            {
                var attsToRemove = await messagesRepository.RemoveListAsync(ids, userId);
                if(attsToRemove != null && attsToRemove.Count > 0)
                {
                    foreach(var attachment in attsToRemove)
                    {
                        FilesHelper.DeleteFile(attachment.Src);
                        FilesHelper.DeleteFile(attachment.CompressedSrc);
                    }
                }
                return DataServiceResult.Success();
            }
            catch(Exception e)
            {
                logger.Error($"Ошибка при удалении сообщений: {e.Message}");
                return DataServiceResult.Failed("Внутренняя ошибка");
            }
        }
    }
}