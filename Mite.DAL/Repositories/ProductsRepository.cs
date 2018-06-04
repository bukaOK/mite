using AutoMapper;
using Dapper;
using Mite.CodeData.Enums;
using Mite.DAL.Core;
using Mite.DAL.DTO;
using Mite.DAL.Entities;
using Mite.DAL.Filters;
using Mite.DAL.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.DAL.Repositories
{
    public class ProductsRepository : Repository<Product>
    {
        public ProductsRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<(double min, double max)> GetMinMaxPricesAsync()
        {
            var min = await DbContext.Posts.Where(x => x.Type == PostTypes.Published).MinAsync(x => (double?)x.Product.Price);
            var max = await DbContext.Posts.Where(x => x.Type == PostTypes.Published).MaxAsync(x => (double?)x.Product.Price);
            return (min: min ?? 0, max: max ?? 0);
        }
        public async Task<IEnumerable<ProductDTO>> GetForUserAsync(string userId, SortFilter sort)
        {
            var query = "select posts.*, (select count(*) from dbo.\"Comments\" as comments where comments.\"PostId\"=posts.\"Id\") " +
                "as \"CommentsCount\", posts.*, products.*, tags.* from dbo.\"Posts\" as posts " +
                "left outer join dbo.\"Products\" products on products.\"Id\"=posts.\"ProductId\" " +
                "left outer join dbo.\"TagPosts\" as tag_posts on tag_posts.\"Post_Id\"=posts.\"Id\" " +
                "left outer join dbo.\"Tags\" as tags on tags.\"Id\"=tag_posts.\"Tag_Id\" " +
                "where posts.\"ProductId\" is not null and posts.\"UserId\"=@userId ";
            switch (sort)
            {
                case SortFilter.Popular:
                    query += "order by posts.\"Rating\" desc";
                    break;
                case SortFilter.New:
                    query += "order by posts.\"PublishDate\" desc";
                    break;
                case SortFilter.Old:
                    query += "order by posts.\"PublishDate\" asc";
                    break;
                case SortFilter.Cheap:
                    query += "order by products.\"Price\" asc";
                    break;
                case SortFilter.Expensive:
                    query += "order by products.\"Price\" desc";
                    break;
                default:
                    query += "order by posts.\"PublishDate\" desc";
                    break;
            }
            query += ";";
            var products = new List<ProductDTO>();
            await Db.QueryAsync<PostDTO, Product, Tag, PostDTO>(query, (post, product, tag) =>
            {
                var dto = products.FirstOrDefault(x => x.Id == product.Id);
                if (dto == null)
                {
                    dto = Mapper.Map<ProductDTO>(post);
                    dto.Id = product.Id;
                    dto.Price = product.Price;
                    dto.Tags = new List<Tag>();
                    if (tag != null)
                        dto.Tags.Add(tag);
                    products.Add(dto);
                }
                else if (tag != null)
                    dto.Tags.Add(tag);
                return post;
            }, new { userId });
            return products;
        }
        public async Task<IEnumerable<ProductDTO>> GetByFilterAsync(ProductTopFilter filter)
        {
            var query = "select posts.\"Id\" from dbo.\"Posts\" as posts inner join dbo.\"Products\" products " +
                "on products.\"Id\"=posts.\"ProductId\" inner join dbo.\"Users\" users on users.\"Id\"=posts.\"UserId\" " +
                $"where posts.\"Type\"='{(byte)PostTypes.Published}' and posts.\"PublishDate\" is not null and " +
                "posts.\"PublishDate\" > @MinDate and posts.\"PublishDate\" < @MaxDate ";
            if (filter.ForAuthors)
                query += "and products.\"ForAuthors\"=true ";
            if (!string.IsNullOrEmpty(filter.Input))
            {
                query += "and (setweight(to_tsvector('mite_ru', posts.\"Title\"), 'A') || " +
                    "setweight(to_tsvector('mite_ru', coalesce(posts.\"Description\", '')), 'B')) @@ plainto_tsquery(@Input) ";
            }
            if (filter.Tags != null && filter.Tags.Length > 0)
            {
                var tagNamesStr = new StringBuilder();
                for (var i = 0; i < filter.Tags.Length; i++)
                {
                    tagNamesStr.AppendFormat("tags.\"Name\"='{0}' ", filter.Tags[i]);
                    if (i < filter.Tags.Length - 1)
                        tagNamesStr.Append("or ");
                }
                //Запрос для получения кол-ва совпадений тегов для каждого поста
                query += "and posts.\"Id\" in (select tag_posts.\"Post_Id\" from dbo.\"Tags\" as tags " +
                        "inner join dbo.\"TagPosts\" as tag_posts on tags.\"Id\"=tag_posts.\"Tag_Id\" " +
                        //Чтобы у поста было точное кол-во совпадений с тегами(т.е. написали в запросе 2 тега - должно совпасть 2 тега или больше)
                        $"where {tagNamesStr.ToString()} group by tag_posts.\"Post_Id\" having count(tag_posts.\"Tag_Id\")>={filter.Tags.Length}) ";
            }

            if (filter.MinPrice > 0)
                query += "and products.\"Price\" > @MinPrice ";
            if (filter.MaxPrice > 0)
                query += "and products.\"Price\" < @MaxPrice ";

            if (filter.CityId != null)
                query += "and users.\"CityId\"=@CityId ";

            var sortQuery = "order by posts.\"PublishDate\" desc";
            switch (filter.Sort)
            {
                case ProductFilter.Cheap:
                    sortQuery = "order by products.\"Price\" asc";
                    break;
                case ProductFilter.Expensive:
                    sortQuery = "order by products.\"Price\" desc";
                    break;
                case ProductFilter.Popular:
                    sortQuery = "order by posts.\"Rating\" desc";
                    break;
            }
            query += $"{sortQuery} limit {filter.Range} offset {filter.Offset}";
            filter.PostIds = (await Db.QueryAsync<Guid>(query, filter)).ToList();

            query = "select posts.*, (select count(*) from dbo.\"Comments\" as comments where comments.\"PostId\"=posts.\"Id\") " +
                "as \"CommentsCount\", users.*, products.*, tags.* from dbo.\"Posts\" as posts " +
                "inner join dbo.\"Users\" as users on posts.\"UserId\"=users.\"Id\" " +
                "left outer join dbo.\"Products\" products on products.\"Id\"=posts.\"ProductId\" " +
                "left outer join dbo.\"TagPosts\" as tag_posts on tag_posts.\"Post_Id\"=posts.\"Id\" " +
                "left outer join dbo.\"Tags\" as tags on tags.\"Id\"=tag_posts.\"Tag_Id\" " +
                $"where posts.\"Id\"=any(@PostIds) {sortQuery};";
            //$"where posts.\"Id\"=any(@PostIds) {sortQuery};";

            var products = new List<ProductDTO>();
            await Db.QueryAsync<PostDTO, User, Product, Tag, PostDTO>(query, (post, user, product, tag) =>
            {
                var dto = products.FirstOrDefault(x => x.Id == post.Id);
                if (dto == null)
                {
                    dto = Mapper.Map<ProductDTO>(post);
                    dto.Price = product.Price;
                    dto.User = user;
                    dto.Tags = new List<Tag>();
                    if (tag != null)
                        dto.Tags.Add(tag);
                    products.Add(dto);
                }
                else if (tag != null)
                    dto.Tags.Add(tag);
                return post;
            }, filter);
            return products;
        }
        /// <summary>
        /// Получить покупку
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="buyerId"></param>
        /// <returns></returns>
        public async Task<Purchase> GetPurchaseAsync(Guid productId, string buyerId)
        {
            return await DbContext.Purchases.FirstOrDefaultAsync(x => x.ProductId == productId && x.BuyerId == buyerId);
        }
        public async Task AddPurchaseAsync(Purchase purchase)
        {
            DbContext.Purchases.Add(purchase);
            await SaveAsync();
        }
    }
}
