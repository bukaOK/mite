using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.BLL.DTO
{
    public class CommentDTO
    {
        public Guid Id { get; set; }
        /// <summary>
        /// Когда комментарий опубликован
        /// </summary>
        public DateTime PublicTime { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
        /// <summary>
        /// Оценка текущего юзера
        /// </summary>
        public RatingDTO CurrentRating { get; set; }
        public List<RatingDTO> Ratings { get; set; }
        public string UserId { get; set; }
        public Guid? ParentCommentId { get; set; }
        public Guid? PostId { get; set; }
        public UserDTO User { get; set; }
        public CommentDTO ParentComment { get; set; }
        public PostDTO Post { get; set; }
    }
}
