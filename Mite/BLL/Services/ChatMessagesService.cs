﻿using Mite.BLL.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
using System.IO;
using Newtonsoft.Json;
using System.Text;

namespace Mite.BLL.Services
{
    public interface IChatMessagesService : IDataService
    {
        Task<IEnumerable<ChatMessageModel>> GetByFilterAsync(Guid chatId, string userId, int page, int range);
        Task<DataServiceResult> AddAsync(ChatMessageModel model);
        Task<DataServiceResult> RemoveListAsync(Guid[] ids, string userId);
        Task<DataServiceResult> ReadAsync(Guid id, string userId);
    }
    public class ChatMessagesService : DataService, IChatMessagesService
    {
        private const string SecKey = "AOgdKZhNixsujiLUWhqmeCYTqu7FWB5Q";
        private const string AttachmentsImagesFolder = "~/Public/attachments/images";
        private const string AttachmentsDocumentsFolder = "~/Public/attachments/documents";

        private readonly ChatMessagesRepository messagesRepository;
        private readonly ChatRepository chatRepository;
        private readonly AppUserManager userManager;
        private readonly ILogger logger;

        public ChatMessagesService(IUnitOfWork database, AppUserManager userManager, ILogger logger) : base(database)
        {
            messagesRepository = database.GetRepo<ChatMessagesRepository, ChatMessage>();
            chatRepository = database.GetRepo<ChatRepository, Chat>();
            this.userManager = userManager;
            this.logger = logger;
        }

        public async Task<DataServiceResult> AddAsync(ChatMessageModel model)
        {
            var chat = await chatRepository.GetWithMembersAsync(model.ChatId);
            if(chat == null)
                return DataServiceResult.Failed("Неизвестный диалог");
            if (!chat.Members.Any(x => x.Id == model.Sender.Id))
                return DataServiceResult.Failed("Неизвестный пользователь");

            var key = CryptoHelper.CreateKey(chat.Id.ToString() + model.Sender.Id + SecKey);
            var encrypted = await CryptoHelper.EncryptAsync(model.Message, key);
            var message = new ChatMessage
            {
                ChatId = chat.Id,
                IV = encrypted.iv,
                Content = encrypted.data,
                SenderId = model.Sender.Id,
                SendDate = DateTime.UtcNow,
                Recipients = chat.Members.Select(x =>
                {
                    var re = new ChatMessageUser
                    {
                        UserId = x.Id
                    };
                    if (x.Id == model.Sender.Id)
                        re.Read = true;

                    return re;
                }).ToList()
            };
            if(model.Attachments.Count > 0)
            {
                message.Attachments = model.Attachments.Select(attModel =>
                {
                    var att = new ChatMessageAttachment
                    {
                        Type = FilesHelper.AttachmentTypeBase64(attModel.Src),
                        MessageId = message.Id,
                        Name = attModel.Name.Length <= 150 ? attModel.Name : $"{attModel.Name.Substring(0, 146)}.{attModel.Name.Split('.').Last()}"
                    };
                    switch (att.Type)
                    {
                        case AttachmentTypes.Image:
                            att.Src = FilesHelper.CreateImage(AttachmentsImagesFolder, attModel.Src);
                            ImagesHelper.Compressed.Compress(HostingEnvironment.MapPath(att.Src));
                            att.CompressedSrc = ImagesHelper.Compressed.CompressedVirtualPath(HostingEnvironment.MapPath(att.Src));
                            break;
                        default:
                            try
                            {
                                var ext = att.Name.Split('.').Last();
                                if(!string.IsNullOrEmpty(ext))
                                    att.Src = FilesHelper.CreateDocument(AttachmentsDocumentsFolder, attModel.Src, ext);
                                else
                                    att.Src = FilesHelper.CreateDocument(AttachmentsDocumentsFolder, attModel.Src);
                            }
                            catch (InvalidOperationException)
                            {
                                att.Src = FilesHelper.CreateDocument(AttachmentsDocumentsFolder, attModel.Src);
                            }
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
                model.Attachments = Mapper.Map<IList<MessageAttachmentModel>>(message.Attachments);
                model.Recipients = chat.Members.Select(x => new UserShortModel
                {
                    Id = x.Id
                }).ToList();
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
            if (!chat.Members.Any(x => x.Id == userId))
                throw new ArgumentException("Неизвестный пользователь");
            var offset = (page - 1) * range;

            var messages = await messagesRepository.GetAsync(chatId, range, offset, userId);
            
            return Mapper.Map<IEnumerable<ChatMessageModel>>(messages);
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

        public async Task<DataServiceResult> RemoveListAsync(Guid[] ids, string userId)
        {
            try
            {
                await messagesRepository.RemoveListAsync(ids, userId);
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