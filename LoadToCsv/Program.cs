using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadToCsv
{
    class Program
    {
        static void Main(string[] args)
        {
            using(var connection = new SqlConnection(@"Data Source=194.87.103.114\SQLEXPRESS,1433;Initial Catalog=MiteDb;User ID=Buka;Password=Evd$utTC"))
            {
                var posts = connection.Query<Post>("select * from dbo.Posts");
                var comments = connection.Query<Comment>("select Id,PublicTime,Content,Rating,UserId,ParentCommentId,PostId from dbo.Comments");

                using (var writer = File.CreateText(@"D:\specify\posts.csv"))
                {
                    foreach (var post in posts)
                    {
                        var publishDtStr = post.PublishDate == null ? "NULL" : post.PublishDate.Value.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        var descrStr = string.IsNullOrEmpty(post.Description) ? "NULL" : $"\"{post.Description.Replace("\"", "")}\"";
                        var row = $"\"{post.Id.ToString()}\"|\"{post.Title}\"|\"{post.Content}\"|{post.IsImage}|{post.LastEdit.ToString("yyyy-MM-dd HH:mm:ss.fff")}|{descrStr}|{post.Rating}|{post.Views}" +
                            $"|{post.UserId}|{post.Cover ?? "NULL"}|{post.IsPublished}|{publishDtStr}|{post.Blocked}";
                        writer.WriteLine(row);
                    }
                }
                using (var writer = File.CreateText(@"D:\specify\comments.csv"))
                {
                    foreach(var comment in comments)
                    {
                        var parentComStr = comment.ParentComment == null ? "NULL" : comment.ParentComment.ToString();
                        var postIdStr = comment.PostId == null ? "NULL" : comment.PostId.ToString();
                        var row = $"{comment.Id}|{comment.PublicTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}|\"{comment.Content.Replace("\"", "")}\"|{comment.Rating}" +
                            $"|{comment.UserId}|{parentComStr}|{postIdStr}";
                        writer.WriteLine(row);
                    }
                }
            }
        }
    }
    public class Post
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        /// <summary>
        /// Может храниться путь к картинке, если пост является рисунком
        /// или текст, если это рассказ, книга и т.п.
        /// </summary>
        public string Content { get; set; }
        public bool IsImage { get; set; }
        /// <summary>
        /// Время последнего редактирования
        /// </summary>
        public DateTime LastEdit { get; set; }
        /// <summary>
        /// Когда опубликовано
        /// </summary>
        public DateTime? PublishDate { get; set; }
        public bool IsPublished { get; set; }
        public bool Blocked { get; set; }
        /// <summary>
        /// Путь к обложке
        /// </summary>
        public string Cover { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// Для экономии добавляем int'овое значение рейтинга
        /// </summary>
        public int Rating { get; set; }
        /// <summary>
        /// Кол-во просмотров
        /// </summary>
        public int Views { get; set; }
        public string UserId { get; set; }
    }
    public class Comment
    {
        public Guid Id { get; set; }
        /// <summary>
        /// Когда комментарий опубликован
        /// </summary>
        public DateTime PublicTime { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
        public string UserId { get; set; }
        public Guid? ParentCommentId { get; set; }
        public Guid? PostId { get; set; }
        public Comment ParentComment { get; set; }
    }
}
