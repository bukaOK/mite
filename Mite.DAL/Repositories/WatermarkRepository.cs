using Mite.CodeData.Enums;
using Mite.DAL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Mite.DAL.Repositories
{
    public class WatermarkRepository : Repository<Watermark>
    {
        public WatermarkRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
        /// <summary>
        /// Существуют ли водяные знаки с тем же путем, что и у искомого
        /// </summary>
        /// <param name="watId"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<bool> SameExistsAsync(Guid watId, string path)
        {
            return Table.AnyAsync(x => x.Id != watId && x.VirtualPath == path);
        }
        /// <summary>
        /// Получить кол-во работ, которые используют этот водяной знак
        /// </summary>
        /// <param name="watermarkId"></param>
        /// <returns></returns>
        public async Task<int> GetPostsCountAsync(Guid watermarkId)
        {
            return await DbContext.Posts.CountAsync(x => x.WatermarkId == watermarkId);
        }
        public async Task<IList<Watermark>> GetByPathAsync(string path)
        {
            return await Table.Where(x => x.VirtualPath == path && x.ImageHash != null).ToListAsync();
        }
        public Task<Watermark> GetByHashAsync(string hash)
        {
            return Table.FirstOrDefaultAsync(x => x.ImageHash == hash);
        }
        public Task<Watermark> GetByParamsAsync(string hash, ImageGravity gravity)
        {
            return Table.FirstOrDefaultAsync(x => x.ImageHash == hash && x.Gravity == gravity);
        }
        public Task<Watermark> GetByParamsAsync(string text, ImageGravity gravity, int fontSize, bool inverted)
        {
            return Table.FirstOrDefaultAsync(x => x.Text == text && x.Gravity == gravity 
                && x.FontSize == fontSize && x.Invert == inverted);
        }
    }
}
