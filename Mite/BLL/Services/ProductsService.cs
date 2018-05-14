using AutoMapper;
using Mite.BLL.Core;
using Mite.BLL.Helpers;
using Mite.BLL.IdentityManagers;
using Mite.CodeData.Enums;
using Mite.DAL.Entities;
using Mite.DAL.Filters;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using Mite.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Mite.BLL.Services
{
    public interface IProductsService : IDataService
    {
        Task<DataServiceResult> AddAsync(ProductEditModel editModel);
        Task<DataServiceResult> UpdateAsync(ProductEditModel editModel);
        Task<IEnumerable<ProductTopModel>> GetTopAsync(ProductTopFilterModel filterModel);
        /// <summary>
        /// Получить товары пользователя
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        Task<IEnumerable<ProductTopModel>> GetForUserAsync(string userName, SortFilter sort);
        Task<DataServiceResult> RemoveAsync(Guid id);
        ProductEditModel GetForEdit(Guid id);
        Task<DataServiceResult> BuyAsync(Guid productId, string userId);
        Task<ProductTopFilterModel> GetTopModelAsync();
        Task<DataServiceResult> ConfirmPurchase(string email, string code);
    }
    public class ProductsService : DataService, IProductsService
    {
        const string ImagesFolder = "/upload/images/products/";
        const string BonusFolder = "/upload/bonuses/archives/";

        private readonly ProductsRepository productsRepository;
        private readonly AppUserManager userManager;
        private readonly ICashService cashService;
        private readonly ITagsService tagsService;

        public ProductsService(IUnitOfWork database, ILogger logger, AppUserManager userManager, ICashService cashService, 
            ITagsService tagsService) : base(database, logger)
        {
            productsRepository = Database.GetRepo<ProductsRepository, Product>();
            this.userManager = userManager;
            this.cashService = cashService;
            this.tagsService = tagsService;
        }

        public async Task<DataServiceResult> AddAsync(ProductEditModel editModel)
        {
            try
            {
                var product = Mapper.Map<Product>(editModel);
                product.Id = Guid.NewGuid();

                //Валидация бонуса происходит в контроллере
                if (!string.IsNullOrEmpty(editModel.BonusBase64))
                {
                    product.BonusPath = FilesHelper.CreateFile(BonusFolder, editModel.BonusBase64, editModel.BonusFormat);
                }
                await productsRepository.AddAsync(product);
                return DataServiceResult.Success(product.Id);
            }
            catch (Exception e)
            {
                return CommonError("Ошибка при создании", e);
            }
        }

        public async Task<DataServiceResult> BuyAsync(Guid productId, string userId)
        {
            var existingPurchase = await productsRepository.GetPurchaseAsync(productId, userId);
            if (existingPurchase != null)
                return DataServiceResult.Failed("Товар уже куплен");
            else
            {
                var cash = await cashService.GetUserCashAsync(userId);
                var product = await productsRepository.GetAsync(productId);
                var post = await Database.GetRepo<PostsRepository, Post>().GetByProductAsync(productId);
                if(cash < product.Price)
                    return DataServiceResult.Failed("Недостаточно средств");
                using(var transaction = productsRepository.BeginTransaction())
                {
                    try
                    {
                        await cashService.AddAsync(userId, post.UserId, product.Price, CashOperationTypes.Purchase);
                        await productsRepository.AddPurchaseAsync(new Purchase
                        {
                            ProductId = productId,
                            BuyerId = userId,
                            Date = DateTime.UtcNow
                        });
                        transaction.Commit();
                        return Success;
                    }
                    catch(Exception e)
                    {
                        transaction.Rollback();
                        return CommonError("Ошибка при покупке", e);
                    }
                }
            }
        }

        public Task<DataServiceResult> ConfirmPurchase(string email, string code)
        {
            throw new NotImplementedException();
        }

        public ProductEditModel GetForEdit(Guid id)
        {
            var product = productsRepository.Get(id);
            return Mapper.Map<ProductEditModel>(product);
        }

        public async Task<IEnumerable<ProductTopModel>> GetForUserAsync(string userName, SortFilter sort)
        {
            var user = await userManager.FindByNameAsync(userName);
            var products = await productsRepository.GetForUserAsync(user.Id, sort);
            return Mapper.Map<IEnumerable<ProductTopModel>>(products);
        }

        public async Task<IEnumerable<ProductTopModel>> GetTopAsync(ProductTopFilterModel filterModel)
        {
            const int range = 30;
            var filter = Mapper.Map<ProductTopFilter>(filterModel);

            filter.Range = range;
            filter.Offset = range * (filterModel.Page - 1);

            var result = await productsRepository.GetByFilterAsync(filter);
            var user = string.IsNullOrEmpty(filterModel.CurrentUserId) ? null : await userManager.FindByIdAsync(filterModel.CurrentUserId);
            return Mapper.Map<IEnumerable<ProductTopModel>>(result, opts => opts.Items.Add("currentUser", user));
        }

        public async Task<ProductTopFilterModel> GetTopModelAsync()
        {
            var citiesRepo = Database.GetRepo<CitiesRepository, City>();
            var model = new ProductTopFilterModel
            {
                Tags = (await tagsService.GetWithPopularityAsync(true, 30)).Select(x => x.Name),
                Cities = (await citiesRepo.GetAllAsync()).Select(city => new SelectListItem
                {
                    Text = city.Name,
                    Value = city.Id.ToString()
                })
            };
            var (min, max) = await productsRepository.GetMinMaxPricesAsync();
            model.MinPrice = (int)min;
            model.MaxPrice = (int)max;

            return model;
        }

        public async Task<DataServiceResult> RemoveAsync(Guid id)
        {
            try
            {
                var product = await productsRepository.GetAsync(id);
                if (product == null)
                    return DataServiceResult.Failed("Товар не найден");
                if (!string.IsNullOrEmpty(product.BonusPath))
                    FilesHelper.DeleteFile(product.BonusPath);
                await productsRepository.RemoveAsync(product);
                return Success;
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при удалении продукта", e);
            }
        }

        public async Task<DataServiceResult> UpdateAsync(ProductEditModel editModel)
        {
            if (editModel.Id == null || editModel.Id == Guid.Empty)
                return DataServiceResult.Failed("Неверный Id товара");
            var existingProduct = await productsRepository.GetAsync((Guid)editModel.Id);
            if(string.IsNullOrEmpty(editModel.BonusBase64))
            {
                FilesHelper.DeleteFile(existingProduct.BonusPath);
                existingProduct.BonusPath = null;
            }
            else if(editModel.BonusBase64 != existingProduct.BonusPath)
            {
                FilesHelper.DeleteFile(existingProduct.BonusPath);
                existingProduct.BonusPath = FilesHelper.CreateFile(BonusFolder, editModel.BonusBase64, editModel.BonusFormat);
            }
            //В контроллере валидация не пропустит пустое описание и непустой файл
            existingProduct.BonusDescription = editModel.BonusDescription;
            existingProduct.Price = editModel.Price ?? 0;

            try
            {
                await productsRepository.UpdateAsync(existingProduct);
                return Success;
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при обновлении", e);
            }
        }
    }
}