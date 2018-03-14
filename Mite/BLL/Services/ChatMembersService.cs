using Mite.BLL.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;
using NLog;
using Mite.CodeData.Enums;
using Mite.DAL.Repositories;
using Mite.DAL.Entities;
using Mite.Models;
using AutoMapper;

namespace Mite.BLL.Services
{
    public interface IChatMembersService : IDataService
    {
        Task<IEnumerable<ChatMemberModel>> GetByChatAsync(Guid chatId, string currentUserId);
        /// <summary>
        /// Добавить пользователя в чат
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="currentUserId"></param>
        /// <param name="chatId"></param>
        /// <returns></returns>
        Task<DataServiceResult> AddAsync(string userId, string currentUserId, Guid chatId);
        /// <summary>
        /// Войти в чат
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="chatId"></param>
        /// <returns></returns>
        Task<DataServiceResult> EnterAsync(string userId, Guid chatId);
        /// <summary>
        /// Пользователя исключают из чата
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="memberId"></param>
        /// <param name="currentUserId"></param>
        /// <returns></returns>
        Task<DataServiceResult> ExcludeAsync(Guid chatId, string memberId, string currentUserId);
        /// <summary>
        /// Пользователь выходит из чата
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<DataServiceResult> ExitAsync(Guid chatId, string userId);
    }
    public class ChatMembersService : DataService, IChatMembersService
    {
        private readonly ChatMembersRepository membersRepository;

        public ChatMembersService(IUnitOfWork database, ILogger logger) : base(database, logger)
        {
            membersRepository = database.GetRepo<ChatMembersRepository, ChatMember>();
        }

        public async Task<DataServiceResult> AddAsync(string userId, string currentUserId, Guid chatId)
        {
            var isFollower = await Database.GetRepo<FollowersRepository, Follower>().IsFollowerAsync(userId, currentUserId);
            if (!isFollower)
                return DataServiceResult.Failed("Можно добавлять только подписчиков");

            var isMember = await membersRepository.IsMemberAsync(chatId, currentUserId);
            if (!isMember)
                return DataServiceResult.Failed("Неизвестный пользователь");
            var chat = await Database.GetRepo<ChatRepository, Chat>().GetAsync(chatId);
            var membersCount = await membersRepository.GetCountAsync(chatId);
            if (chat.MaxMembersCount > 0 && ++membersCount > chat.MaxMembersCount)
                return DataServiceResult.Failed("Достигнуто максимальное кол-во участников");
            try
            {
                var member = await membersRepository.GetAsync(userId, chatId);
                if(member == null)
                    await membersRepository.AddAsync(new ChatMember
                    {
                        UserId = userId,
                        ChatId = chatId,
                        InviterId = currentUserId,
                        EnterDate = DateTime.UtcNow
                    });
                else
                {
                    if (member.UserId == currentUserId && member.Status == ChatMemberStatuses.Excluded)
                        return DataServiceResult.Failed("Действие запрещено");
                    if (member.UserId != currentUserId && member.Status == ChatMemberStatuses.CameOut)
                        return DataServiceResult.Failed("Пользователь вышел из чата");
                    member.Status = ChatMemberStatuses.InChat;
                    member.InviterId = currentUserId == userId ? null : currentUserId;
                    await membersRepository.UpdateAsync(member);
                }
                return Success;
            }
            catch (Exception e)
            {
                return CommonError("Ошибка при добавлении участников чата", e);
            }
        }

        public async Task<DataServiceResult> EnterAsync(string userId, Guid chatId)
        {
            var chat = await Database.GetRepo<ChatRepository, Chat>().GetWithLastMessageAsync(chatId);
            var member = await membersRepository.GetAsync(userId, chatId);
            var shortChat = Mapper.Map<ShortChatModel>(chat);
            if (chat == null)
                return DataServiceResult.Failed("Неизвестный чат");
            if(member != null && member.Status == ChatMemberStatuses.CameOut)
            {
                try
                {
                    member.Status = ChatMemberStatuses.InChat;
                    await membersRepository.UpdateAsync(member);
                    return DataServiceResult.Success(shortChat);
                }
                catch(Exception e)
                {
                    return CommonError("Ошибка при входе в чат", e);
                }
            }
            if(member == null && chat.Type == ChatTypes.Public)
            {
                try
                {
                    member = new ChatMember
                    {
                        Status = ChatMemberStatuses.InChat,
                        EnterDate = DateTime.UtcNow,
                        UserId = userId,
                        ChatId = chat.Id,
                    };
                    await membersRepository.AddAsync(member);
                    return DataServiceResult.Success(shortChat);
                }
                catch (Exception e)
                {
                    return CommonError("Ошибка при входе в чат", e);
                }
            }
            return DataServiceResult.Failed("Действие запрещено");
        }

        public async Task<DataServiceResult> ExcludeAsync(Guid chatId, string memberId, string currentUserId)
        {
            var chat = await Database.GetRepo<ChatRepository, Chat>().GetAsync(chatId);
            var member = await membersRepository.GetAsync(memberId, chatId);
            if (member == null || member.Status != ChatMemberStatuses.InChat && member.InviterId != currentUserId && chat.CreatorId != currentUserId)
                return DataServiceResult.Failed("Действие запрещено");
            try
            {
                member.Status = ChatMemberStatuses.Excluded;
                await membersRepository.UpdateAsync(member);
                return Success;
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при исключении участника чата", e);
            }
        }

        public async Task<DataServiceResult> ExitAsync(Guid chatId, string userId)
        {
            var member = await membersRepository.GetAsync(chatId, userId);
            if (member == null || member.Status != ChatMemberStatuses.InChat)
                return DataServiceResult.Failed("Пользователь не является участником чата");
            try
            {
                member.Status = ChatMemberStatuses.CameOut;
                await membersRepository.UpdateAsync(member);
            }
            catch(Exception e)
            {
                return CommonError("Ошибка во время выхода участника чата", e);
            }
            return Success;
        }

        public async Task<IEnumerable<ChatMemberModel>> GetByChatAsync(Guid chatId, string currentUserId)
        {
            var isMember = await membersRepository.IsMemberAsync(chatId, currentUserId);
            if (!isMember)
                return null;
            var members = await membersRepository.GetByChatAsync(chatId);
            return Mapper.Map<IEnumerable<ChatMemberModel>>(members, opts =>
            {
                opts.Items.Add("currentUserId", currentUserId);
            });
        }
    }
}