﻿using Mite.BLL.Core;
using System;
using Mite.DAL.Infrastructure;
using System.Threading.Tasks;
using Mite.DAL.Entities;
using NLog;
using Mite.DAL.Repositories;
using Mite.Models;
using System.Collections.Generic;
using Mite.CodeData.Enums;
using AutoMapper;
using Mite.BLL.Helpers;
using Mite.CodeData.Constants;
using Mite.ExternalServices.VkApi.Wall;
using System.Net.Http;
using System.Linq;
using Mite.BLL.IdentityManagers;
using Newtonsoft.Json;
using Mite.BLL.DTO;
using Microsoft.AspNet.Identity;
using System.Web;
using Mite.ExternalServices.VkApi.Messages;
using System.Web.Hosting;

namespace Mite.BLL.Services
{
    public interface IDealService : IDataService
    {
        /// <summary>
        /// Создание сделки к услуге
        /// </summary>
        /// <param name="authorServiceId">Id услуги</param>
        /// <param name="clientId">Id заказчика</param>
        /// <returns></returns>
        Task<DataServiceResult> CreateAsync(Guid authorServiceId, string clientId);
        /// <summary>
        /// Создание сделки к заказу
        /// </summary>
        /// <param name="orderId">Id заказа</param>
        /// <param name="executerId">Id исполнителя(автора)</param>
        /// <param name="callerId">Id делающего запрос(для проверки)</param>
        /// <returns></returns>
        Task<DataServiceResult> CreateAsync(Guid orderId, string executerId, string callerId);
        Task<DataServiceResult> RemoveAsync(long id);
        /// <summary>
        /// Обновление(новая сделка)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<DataServiceResult> UpdateNewAsync(DealModel model, string userId);
        /// <summary>
        /// Сохранение результата изображения(3 этап)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="imageBase64"></param>
        /// <param name="authorId"></param>
        /// <returns></returns>
        Task<DataServiceResult> SaveResultImgAsync(long id, string imageBase64, string authorId);
        /// <summary>
        /// Подтверждение клиентом
        /// </summary>
        /// <param name="id">Id сделки</param>
        /// <param name="clientId">Id клиента</param>
        /// <param name="confirm">Подтверждает или отклоняет</param>
        /// <returns></returns>
        Task<DataServiceResult> ConfirmAsync(long id, string clientId = null);
        Task<DataServiceResult> RejectAsync(long id, string userId = null);
        /// <summary>
        /// Открыть спор
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<DataServiceResult> OpenDisputeAsync(long id, string userId);
        Task<DealModel> GetShowAsync(long id, string currentUserId);
        Task<IEnumerable<DealUserModel>> GetIncomingAsync(DealStatuses dealType, string authorId);
        Task<IEnumerable<DealUserModel>> GetOutgoingAsync(DealStatuses dealType, string clientId);
        Task<IEnumerable<DealUserModel>> GetForModerAsync(DealStatuses status, string moderId);
        /// <summary>
        /// Оплата сделки клиентом
        /// </summary>
        /// <param name="id"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        Task<DataServiceResult> PayAsync(long id, string clientId);
        /// <summary>
        /// Проверка, репостнул ли клиент запись
        /// </summary>
        /// <param name="id"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        Task<DataServiceResult> CheckVkRepostAsync(long id, string clientId);
        /// <summary>
        /// Автор сам подтверждает репост его записи
        /// </summary>
        /// <param name="id"></param>
        /// <param name="authorId"></param>
        /// <returns></returns>
        Task<DataServiceResult> ConfirmVkRepostAsync(long id, string authorId);
        /// <summary>
        /// Оценка сделки клиентом
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        Task<DataServiceResult> RateAsync(long id, byte value, string clientId);
        /// <summary>
        /// Оставить отзыв к сделке
        /// </summary>
        /// <param name="id"></param>
        /// <param name="feedback"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        Task<DataServiceResult> GiveFeedbackAsync(long id, string feedback, string clientId);
        /// <summary>
        /// Кол-во новых заказов автора
        /// </summary>
        /// <returns></returns>
        int GetNewCount(string userId);
    }
    public class DealService : DataService, IDealService
    {
        private readonly IAuthorServiceService authorServiceService;
        private readonly IChatService chatService;
        private readonly ICashService cashService;
        private readonly IExternalServices externalServices;
        private readonly HttpClient httpClient;
        private readonly AppUserManager userManager;
        private readonly DealRepository repo;

        public DealService(IUnitOfWork database, ILogger logger, IAuthorServiceService authorServiceService, IChatService chatService, 
            ICashService cashService, IExternalServices externalServices, HttpClient httpClient, AppUserManager userManager) : base(database, logger)
        {
            this.authorServiceService = authorServiceService;
            this.chatService = chatService;
            this.cashService = cashService;
            this.externalServices = externalServices;
            this.httpClient = httpClient;
            this.userManager = userManager;
            repo = Database.GetRepo<DealRepository, Deal>();
        }

        public async Task<DataServiceResult> CreateAsync(Guid authorServiceId, string clientId)
        {
            var authorService = await Database.GetRepo<AuthorServiceRepository, AuthorService>().GetAsync(authorServiceId);
            if (authorService == null)
                return DataServiceResult.Failed("Не найдена услуга");
            if (authorService.AuthorId == clientId)
                return DataServiceResult.Failed("Вы не можете создавать заказы к своим услугам");
            var deal = new Deal
            {
                ServiceId = authorServiceId,
                ClientId = clientId,
                AuthorId = authorService.AuthorId,
                CreateDate = DateTime.UtcNow,
                Status = DealStatuses.New
            };
            if (!string.IsNullOrEmpty(authorService.VkRepostConditions))
                deal.VkReposted = false;
            return await CreateAsync(deal);
        }
        private async Task<DataServiceResult> CreateAsync(Deal deal)
        {
            var chatRepo = Database.GetRepo<ChatRepository, Chat>();
            try
            {
                var members = new List<string> { deal.AuthorId, deal.ClientId };
                var chat = new Chat
                {
                    Type = ChatTypes.Deal
                };
                chat.Members = members.Select(x => new ChatMember
                {
                    UserId = x,
                    ChatId = chat.Id
                }).ToList();
                await chatRepo.AddAsync(chat);
                deal.ChatId = chat.Id;
                await repo.AddAsync(deal);

                var author = await userManager.FindByIdAsync(deal.AuthorId);
                var logins = await userManager.GetLoginsAsync(deal.AuthorId);
                var vkUserId = logins.FirstOrDefault(x => x.LoginProvider == VkSettings.DefaultAuthType).ProviderKey;
#if !DEBUG
                if (author.MailNotify)
                    await userManager.SendEmailAsync(deal.AuthorId, "Новый заказ!",
                        "По одной из ваших услуг сделали заказ! С уважением, MiteGroup.");
#endif
                if (vkUserId != null)
                {
                    var canVkNotifyReq = new SendingAllowedRequest(httpClient, VkSettings.GroupKey)
                    {
                        GroupId = VkSettings.GroupId,
                        UserId = vkUserId
                    };
                    var canVkNotifyResp = await canVkNotifyReq.PerformAsync();
                    if (canVkNotifyResp.Allowed)
                    {
                        var notifyReq = new SendRequest(httpClient, VkSettings.GroupKey)
                        {
                            UserId = vkUserId,
                            Message = "У Вас новый заказ! Все заказы здесь: https://mitegroup.ru/user/deals/incoming/new. С уважением, MiteGroup."
                        };
                        await notifyReq.PerformAsync();
                    }
                }
                return DataServiceResult.Success(deal.Id);
            }
            catch (Exception e)
            {
                logger.Error("Ошибка при создании сделки: " + e.Message);
                return DataServiceResult.Failed("Ошибка при создании сделки");
            }
        }
        public async Task<IEnumerable<DealUserModel>> GetIncomingAsync(DealStatuses dealType, string authorId)
        {
            var deals = await repo.GetIncomingAsync(dealType, authorId);
            var dealModels = Mapper.Map<IEnumerable<DealUserModel>>(deals);
            return dealModels;
        }

        public int GetNewCount(string userId)
        {
            return userManager.IsInRole(userId, RoleNames.Author) ? repo.GetAuthorCounts(userId) : repo.GetClientCounts(userId);
        }

        public async Task<IEnumerable<DealUserModel>> GetOutgoingAsync(DealStatuses dealType, string clientId)
        {
            var deals = await repo.GetOutgoingAsync(dealType, clientId);
            var dealModels = Mapper.Map<IEnumerable<DealUserModel>>(deals);
            return dealModels;
        }

        public async Task<IEnumerable<DealUserModel>> GetForModerAsync(DealStatuses status, string moderId)
        {
            var deals = await repo.GetForModerAsync(status, moderId);
            if (deals.Count == 0)
                deals = await repo.GetForModerAsync(status, null);
            return Mapper.Map<IEnumerable<DealUserModel>>(deals, opts => opts.Items.Add("forModer", true));
        }
        public async Task<DealModel> GetShowAsync(long id, string currentUserId)
        {
            var currentUser = Mapper.Map<UserShortModel>(await userManager.FindByIdAsync(currentUserId));
            var deal = await repo.GetWithServiceAsync(id);
            if (deal == null)
                throw new HttpException(404, "Not found");
            var dealModel = Mapper.Map<DealModel>(deal);
            if (dealModel.DisputeChat != null)
                dealModel.DisputeChat.CurrentUser = currentUser;
            if (dealModel.Chat != null)
                dealModel.Chat.CurrentUser = currentUser;
            var companion = dealModel.Chat.Members.First(x => !string.Equals(currentUserId, x.Id));
            if(companion != null)
                companion = Mapper.Map<UserShortModel>(await userManager.FindByIdAsync(companion.Id));
            dealModel.Chat.Companion = companion;

            var logins = await userManager.GetLoginsAsync(currentUserId);
            dealModel.VkAuthenticated = logins.Any(x => x.LoginProvider == VkSettings.DefaultAuthType);
            return dealModel;
        }

        public async Task<DataServiceResult> RemoveAsync(long id)
        {
            var deal = await repo.GetAsync(id);
            if (deal == null)
                return DataServiceResult.Failed("Сделка не найдена");
            try
            {
                await repo.RemoveAsync(deal);
                if (!string.IsNullOrEmpty(deal.ImageResultSrc))
                {
                    FilesHelper.DeleteFile(deal.ImageResultSrc);
                    FilesHelper.DeleteFile(deal.ImageResultSrc_50);
                }
                return DataServiceResult.Success();
            }
            catch(Exception e)
            {
                logger.Error($"Id сделки: {deal.Id}. Ошибка при удалении сделки: {e.Message}");
                return DataServiceResult.Failed("Ошибка при удалении сделки");
            }
        }

        public async Task<DataServiceResult> UpdateNewAsync(DealModel model, string userId)
        {
            var deal = await repo.GetAsync(model.Id);
            if (deal == null)
                return DataServiceResult.Failed("Сделка не найден");
            if (deal.Status != DealStatuses.New)
                return DataServiceResult.Failed("Действие запрещено");
            if(deal.AuthorId == userId)
            {
                deal.Deadline = model.Deadline;
                deal.Price = model.Price;
            }
            else if(deal.ClientId == userId)
            {
                deal.Demands = model.Demands;
            }
            else
                return DataServiceResult.Failed("Неизвестный пользователь");

            if (deal.Deadline != null && deal.Price != null && !string.IsNullOrEmpty(deal.Demands))
                deal.Status = model.Status;
            try
            {
                await repo.UpdateAsync(deal);
                return DataServiceResult.Success();
            }
            catch(Exception e)
            {
                logger.Error($"Id: {deal.Id} Ошибка при обновлении сделки: {e.Message}");
                return DataServiceResult.Failed("Ошибка при обновлении сделки");
            }
        }

        public async Task<DataServiceResult> PayAsync(long id, string clientId)
        {
            var deal = await repo.GetAsync(id);
            if (deal == null)
                return DataServiceResult.Failed("Сделка не найдена");
            if (!string.Equals(clientId, deal.ClientId) || !deal.Price.HasValue || !deal.Deadline.HasValue 
                || deal.Payed || deal.Status != DealStatuses.ExpectPayment)
                return DataServiceResult.Failed("Запрещенное действие");

            var userCash = await cashService.GetUserCashAsync(clientId);
            if (userCash < deal.Price)
                return DataServiceResult.Failed("На вашем счету недостаточно средств");
            using(var transaction = repo.BeginTransaction())
            {
                try
                {
                    await cashService.AddAsync(deal.ClientId, null, deal.Price.Value, CashOperationTypes.Deal);
                    deal.Payed = true;

                    var condFilled = await CheckDealConditions(id);
                    if (condFilled)
                        deal.Status = DealStatuses.ExpectClient;

                    await repo.UpdateAsync(deal);
                    transaction.Commit();
                    return DataServiceResult.Success(condFilled);
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    logger.Error($"Id: {deal.Id}; Ошибка при выплате системе за сделку: {e.Message}");
                    return DataServiceResult.Failed("Внутренняя ошибка. Попробуйте позднее");
                }
            }
        }

        public async Task<DataServiceResult> CheckVkRepostAsync(long id, string clientId)
        {
            var deal = await repo.GetAsync(id);
            var authorService = await Database.GetRepo<AuthorServiceRepository, AuthorService>().GetAsync(deal.ServiceId);
            var vkRepost = JsonConvert.DeserializeObject<VkRepostDTO>(authorService.VkRepostConditions);

            if (deal.ClientId != clientId)
                return DataServiceResult.Failed("Неизвестный пользователь");
            if (deal.VkReposted == true || deal.VkReposted == null)
                return DataServiceResult.Failed("Проверка не требуется");

            var vkService = await externalServices.GetAsync(clientId, VkSettings.DefaultAuthType);
            var logins = await userManager.GetLoginsAsync(clientId);
            var vkLogin = logins.FirstOrDefault(x => x.LoginProvider == VkSettings.DefaultAuthType);

            if (vkService == null || vkLogin == null)
                return DataServiceResult.Failed("Не найден ключ доступа. Пожалуйста, выйдите из аккаунта и зайдите через \"ВКонтакте\".");
            
            //функция нахождения профиля пользователя вк(по рекурсии)
            async Task<bool> FindVkRepost(int offset)
            {
                const int count = 1000;
                var req = new GetRepostsRequest(httpClient, vkService.AccessToken)
                {
                    Count = count,
                    OwnerId = vkRepost.OwnerId,
                    PostId = vkRepost.PostId
                };
                var result = await req.PerformAsync();
                var profile = result.Profiles.FirstOrDefault(x => x.Id == vkLogin.ProviderKey);

                if (profile == null)
                {
                    if (result.Items.Count < count)
                        return false;
                    return await FindVkRepost(offset + count);
                }
                return true;
            }
            var repostExists = false;
            try
            {
                repostExists = await FindVkRepost(0);
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при проверке репоста ВКонтакте", e);
            }
            if (repostExists)
            {
                deal.VkReposted = true;
                var condFilled = await CheckDealConditions(id);
                if (condFilled)
                    deal.Status = DealStatuses.ExpectClient;
                try
                {
                    await repo.UpdateAsync(deal);
                    return DataServiceResult.Success(condFilled);
                }
                catch(Exception e)
                {
                    logger.Error($"Id: {deal.Id}. Ошибка при попытке проверки ВК репоста: {e.Message}");
                    return DataServiceResult.Failed("Ошибка при попытке обновления. Повторите попытку позже.");
                }
            }
            return DataServiceResult.Failed("Репост не найден.");
        }

        /// <summary>
        /// Проверяем, все ли условия сделки выполнены
        /// </summary>
        /// <param name="id"></param>
        /// <returns>true - выполнены, false - нет</returns>
        private async Task<bool> CheckDealConditions(long id)
        {
            var deal = await repo.GetAsync(id);
            if (deal.Payed && (deal.VkReposted == null || deal.VkReposted == true))
            {
                return true;
            }
            return false;
        }

        public async Task<DataServiceResult> SaveResultImgAsync(long id, string imageBase64, string authorId)
        {
            var deal = await repo.GetAsync(id);
            if (deal == null)
                return DataServiceResult.Failed("Сделка не найдена");
            if (deal.AuthorId != authorId)
                return DataServiceResult.Failed("Неизвестный пользователь");
            if (string.Equals(deal.ImageResultSrc, imageBase64))
                return DataServiceResult.Failed("Внесите изменения");
            try
            {
                deal.ImageResultSrc = FilesHelper.CreateImage(PathConstants.VirtualImageFolder, imageBase64);
                deal.ImageResultSrc_50 = FilesHelper.ToVirtualPath(ImagesHelper.Resize(HostingEnvironment.MapPath(deal.ImageResultSrc), 500));

                await repo.UpdateAsync(deal);
                return DataServiceResult.Success();
            }
            catch(Exception e)
            {
                logger.Error($"Сделка № {deal.Id}. Ошибка при сохранении результата заказа: {e.Message}");
                return DataServiceResult.Failed("Ошибка при сохранении результата заказа");
            }
        }

        public async Task<DataServiceResult> ConfirmAsync(long id, string clientId = null)
        {
            var isModer = clientId == null;

            var deal = await repo.GetAsync(id);
            if (deal == null)
                return DataServiceResult.Failed("Сделка не найдена");
            if (!isModer && deal.ClientId != clientId)
                return DataServiceResult.Failed("Неизвестный пользователь");
            if (deal.Price != null && !deal.Payed)
                return DataServiceResult.Failed("Сделка не оплачена");
            deal.Status = isModer ? DealStatuses.ModerConfirmed : DealStatuses.Confirmed;
            return await CloseDealAsync(deal);
            
        }
        /// <summary>
        /// Действия по закрытию сделки(обновление надежности и пр.)
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="deal"></param>
        /// <returns></returns>
        private async Task<DataServiceResult> CloseDealAsync(Deal deal)
        {
            var serviceRepo = Database.GetRepo<AuthorServiceRepository, AuthorService>();
            var userRepo = Database.GetRepo<UserRepository, User>();

            var client = await userManager.FindByIdAsync(deal.ClientId);
            var author = await userManager.FindByIdAsync(deal.AuthorId);
            using (var transaction = repo.BeginTransaction())
            {
                try
                {
                    if (deal.Price != null && deal.Payed)
                    {
                        if (deal.Status == DealStatuses.ModerRejected)
                            await cashService.AddAsync(null, deal.ClientId, (double)deal.Price, CashOperationTypes.Deal);
                        else if (deal.Status == DealStatuses.Confirmed || deal.Status == DealStatuses.ModerConfirmed)
                            await cashService.AddAsync(null, deal.AuthorId, (double)deal.Price, CashOperationTypes.Deal);
                        else throw new Exception("Неверный статус сделки");
                    }
                    deal.Payed = false;
                    await repo.UpdateAsync(deal);
                    var resultCoef = 0;
                    switch (deal.Status)
                    {
                        case DealStatuses.Confirmed:
                            resultCoef += DealConstants.GoodCoef;
                            break;
                        case DealStatuses.ModerRejected:
                        case DealStatuses.ModerConfirmed:
                            resultCoef += DealConstants.BadCoef;
                            break;
                    }

                    client.Reliability += resultCoef;
                    author.Reliability += resultCoef;

                    if(deal.ServiceId != null)
                    {
                        var service = await serviceRepo.GetAsync(deal.ServiceId);
                        service.Reliability += resultCoef;
                        await serviceRepo.UpdateAsync(service);
                    }
                    await userManager.UpdateAsync(client);
                    await userManager.UpdateAsync(author);

                    transaction.Commit();
                    return DataServiceResult.Success();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    logger.Error($"Id: {deal.Id}. Ошибка при подтверждении сделки: {e.Message}");
                    return DataServiceResult.Failed("Ошибка при подтверждении");
                }
            }
        }
        public async Task<DataServiceResult> ConfirmVkRepostAsync(long id, string authorId)
        {
            var deal = await repo.GetAsync(id);
            if (deal == null)
                return DataServiceResult.Failed("Сделка не найдена");
            if (deal.AuthorId != authorId)
                return DataServiceResult.Failed("Неизвестный пользователь");
            if (deal.VkReposted == true || deal.VkReposted == null)
                return DataServiceResult.Failed("Подтверждение не требуется");

            deal.VkReposted = true;
            var condFilled = await CheckDealConditions(id);
            if (condFilled)
                deal.Status = DealStatuses.ExpectClient;
            try
            {
                await repo.UpdateAsync(deal);
                return DataServiceResult.Success(condFilled);
            }
            catch(Exception e)
            {
                logger.Error($"Id: {deal.Id}. Ошибка при попытке подтверждения репоста ВК записи сделки: {e.Message}");
                return DataServiceResult.Failed("Ошибка при подтверждении. Попробуйте позже");
            }
        }

        public async Task<DataServiceResult> OpenDisputeAsync(long id, string userId)
        {
            var deal = await repo.GetAsync(id);
            if (deal == null)
                return DataServiceResult.Failed("Сделка не найдена");
            if (deal.ClientId != userId && deal.AuthorId != userId)
                return DataServiceResult.Failed("Неизвестный пользователь");

            deal.Status = DealStatuses.Dispute;

            var chatRepo = Database.GetRepo<ChatRepository, Chat>();
            var disputeChat = new Chat
            {
                Members = new List<ChatMember>
                {
                    new ChatMember { UserId = deal.ClientId },
                    new ChatMember { UserId = deal.AuthorId }
                }
            };
            deal.DisputeChatId = disputeChat.Id;
            using(var transaction = repo.BeginTransaction())
            {
                try
                {
                    await chatRepo.AddAsync(disputeChat);
                    await repo.UpdateAsync(deal);
                    transaction.Commit();
                    return DataServiceResult.Success();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    logger.Error($"Id: {deal.Id}. Ошибка при открытии спора: {e.Message}");
                    return DataServiceResult.Failed("Ошибка при открытии спора");
                }
            }
        }

        public async Task<DataServiceResult> RejectAsync(long id, string userId = null)
        {
            var isModer = userId == null;
            var deal = await repo.GetAsync(id);

            if (!isModer && deal.ClientId != userId && deal.AuthorId != userId)
                return DataServiceResult.Failed("Неизвестный пользователь");
            if(!isModer && deal.Payed)
                return DataServiceResult.Failed("Оплаченную сделку нельзя закрыть");

            deal.Status = isModer ? DealStatuses.ModerRejected : DealStatuses.Rejected;
            return await CloseDealAsync(deal);
        }

        public async Task<DataServiceResult> RateAsync(long id, byte rateVal, string clientId)
        {
            var deal = await repo.GetAsync(id);
            if (deal == null)
                return DataServiceResult.Failed("Сделка не найдена");
            if (deal.ClientId != clientId)
                return DataServiceResult.Failed("Неизвестный пользователь");
            //Выглядит странно, но имхо - так оценки будут более объективными
            if (deal.Status != DealStatuses.ModerRejected && deal.Status != DealStatuses.Confirmed)
                return DataServiceResult.Failed("Действие запрещено");

            try
            {
                await repo.RateAsync(deal, rateVal);
                return DataServiceResult.Success();
            }
            catch(Exception e)
            {
                logger.Error($"Id: {deal.Id}. Ошибка при попытке оценить сделку: {e.Message}");
                return DataServiceResult.Failed("Внутренняя ошибка");
            }
        }

        public async Task<DataServiceResult> GiveFeedbackAsync(long id, string feedback, string clientId)
        {
            var deal = await repo.GetAsync(id);
            if (deal == null)
                return DataServiceResult.Failed("Сделка не найдена");
            if (deal.ClientId != clientId)
                return DataServiceResult.Failed("Неизвестный пользователь");
            if (deal.Status != DealStatuses.ModerRejected && deal.Status != DealStatuses.Confirmed)
                return DataServiceResult.Failed("Действие запрещено");

            deal.Feedback = feedback;
            try
            {
                await repo.UpdateAsync(deal);
                return DataServiceResult.Success();
            }
            catch (Exception e)
            {
                logger.Error($"Id: {deal.Id}. Ошибка при попытке оставить отзыв к сделке: {e.Message}");
                return DataServiceResult.Failed("Внутренняя ошибка");
            }
        }

        public async Task<DataServiceResult> CreateAsync(Guid orderId, string executerId, string callerId)
        {
            var order = await Database.GetRepo<OrderRepository, Order>().GetAsync(orderId);
            if (order == null)
                return DataServiceResult.Failed("Заказ не найден");
            if (order.UserId != callerId)
                return DataServiceResult.Failed("Неизвестный пользователь");
            if (order.UserId == executerId)
                return DataServiceResult.Failed("Вы не можете создавать сделки к своим заказам");
            var deal = new Deal
            {
                OrderId = orderId,
                ClientId = order.UserId,
                AuthorId = executerId,
                CreateDate = DateTime.UtcNow,
                Status = DealStatuses.New
            };
            return await CreateAsync(deal);
        }
    }
}