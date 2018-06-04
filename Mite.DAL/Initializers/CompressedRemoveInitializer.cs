using Mite.DAL.Infrastructure;
using Mite.DAL.Initializers.Core;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Hosting;

namespace Mite.DAL.Initializers
{
    class CompressedRemoveInitializer : DBInitializer
    {
        public CompressedRemoveInitializer(AppDbContext dbContext) : base(dbContext)
        {
        }
        string SavePath => HostingEnvironment.MapPath("/Files/remove_init.txt");
        public override bool Initialized => File.Exists(SavePath);

        public override void Initialize()
        {
            var checkList = new List<string>();
            checkList.AddRange(DbContext.Posts.AsNoTracking().Select(x => x.Content_50).ToList());
            checkList.AddRange(DbContext.ComicsItems.AsNoTracking().Select(x => x.ContentSrc_50).ToList());
            checkList.AddRange(DbContext.PostCollectionItems.AsNoTracking().Select(x => x.ContentSrc_50).ToList());
            checkList.AddRange(DbContext.Orders.AsNoTracking().Select(x => x.ImageSrc_600).ToList());
            checkList.AddRange(DbContext.AuthorServices.AsNoTracking()
                .Select(x => x.ImageSrc_50).ToList().Where(x => !Regex.IsMatch(x, @"^https?:\/")));

            foreach(var item in checkList.Where(x => !string.IsNullOrEmpty(x)))
            {
                var fullPath = HostingEnvironment.MapPath(item);
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
            }
            var str = File.CreateText(SavePath);
            str.Close();
        }
    }
}
