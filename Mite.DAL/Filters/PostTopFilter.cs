using Mite.CodeData.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.DAL.Filters
{
    public class PostTopFilter
    {
        public PostTopFilter(int range, int page)
        {
            Range = range;
            Offset = (page - 1) * range;
        }
        public string PostName { get; set; }
        public string[] Tags { get; set; }
        public Guid[] FollowingTags { get; set; }
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
        public bool OnlyFollowings { get; set; }
        public string CurrentUserId { get; set; }
        public SortFilter SortType { get; set; }
        public int Offset { get; set; }
        public int Range { get; set; }
        public PostTypes PostType { get; set; }
        /// <summary>
        /// Список пользователей, на которых текущий подписан(если OnlyFollowings=true)
        /// </summary>
        public IList<string> Followings { get; set; }
        public IList<Guid> TagPostsIds { get; set; }
        /// <summary>
        /// Список постов для выборки
        /// </summary>
        public IList<Guid> PostIds { get; set; }
    }
}
