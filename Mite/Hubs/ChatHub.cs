﻿using Autofac;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Mite.BLL.IdentityManagers;
using Mite.CodeData.Enums;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using Mite.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Mite.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ILifetimeScope lifetimeScope;
        private readonly ChatMessagesRepository chatMessagesRepository;
        private readonly ChatRepository chatRepository;
        private readonly AppUserManager userManager;

        public ChatHub(ILifetimeScope lifetimeScope)
        {
            this.lifetimeScope = lifetimeScope.BeginLifetimeScope();
            var unitOfWork = new UnitOfWork(new AppDbContext());
            userManager = lifetimeScope.Resolve<AppUserManager>();

            chatMessagesRepository = unitOfWork.GetRepo<ChatMessagesRepository, ChatMessage>();
            chatRepository = unitOfWork.GetRepo<ChatRepository, Chat>();
        }
        /// <summary>
        /// Реагирует на печатание
        /// </summary>
        /// <param name="chatId">Id чата</param>
        /// <param name="beginTyping">Пользователь начал писать(закончил)</param>
        /// <returns></returns>
        public void Typing(Guid chatId, bool beginTyping)
        {
            var userId = Context.User.Identity.GetUserId();
            var userName = Context.User.Identity.Name;
            var chat = chatRepository.GetWithMembers(chatId);

            if (chat == null)
                return;
            if(beginTyping)
                foreach (var member in chat.Members)
                {
                    if(member.UserId != userId && member.Status == ChatMemberStatuses.InChat)
                    {
                        Clients.Group(member.UserId).beginType(chatId, userName);
                    }
                }
            else
                foreach (var member in chat.Members)
                {
                    if(member.UserId != userId)
                    {
                        Clients.Group(member.UserId).endType(chatId);
                    }
                }
        }
        public void AddMessage(ChatMessageModel message)
        {
            if (message.Attachments == null)
                message.Attachments = new List<MessageAttachmentModel>();
            foreach (var re in message.Recipients)
            {
                if (re.Id != message.Sender.Id)
                {
                    Clients.Group(re.Id).addMessage(message);
                }
            }
        }
        public void ReadMessage(Guid messageId)
        {
            var userId = Context.User.Identity.GetUserId();
            chatMessagesRepository.Read(messageId, userId);
            Clients.Group(userId).readMessage(messageId);
        }
        public void ReadMessages(Guid chatId)
        {
            var userId = Context.User.Identity.GetUserId();
            chatMessagesRepository.ReadUnreaded(chatId, userId);
            //Получаем вместе с чатом всех участников
            var chat = chatRepository.GetWithMembers(chatId);
            //Чат может не существовать, если его только создали и он в сессии
            if(chat != null)
                foreach(var member in chat.Members)
                {
                    //делаем у всех участников чата прочитанными
                    if (member.UserId != userId && member.Status != ChatMemberStatuses.Removed)
                    {
                        Clients.Group(member.UserId).readAll(chatId);
                    }
                }
        }
        public override Task OnConnected()
        {
            //В качестве группы у нас пользователь, т.к. он может открыть несколько вкладок
            Groups.Add(Context.ConnectionId, Context.User.Identity.GetUserId());
            return base.OnConnected();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && lifetimeScope != null)
                lifetimeScope.Dispose();
            base.Dispose(disposing);
        }
    }
}