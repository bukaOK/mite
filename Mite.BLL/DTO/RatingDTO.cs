using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.BLL.DTO
{
    public class RatingDTO
    {
        public Guid Id { get; set; }
        public byte Value { get; set; }
        public DateTime RateDate { get; set; }
        public Guid? PostId { get; set; }
        public Guid? CommentId { get; set; }
        public string UserId { get; set; }
        public string OwnerId { get; set; }
        /// <summary>
        /// Кто оценил
        /// </summary>
        public UserDTO User { get; set; }
        /// <summary>
        /// Пост который оценили
        /// </summary>
        public PostDTO Post { get; set; }
        /// <summary>
        /// Комментарий который оценили
        /// </summary>
        public CommentDTO Comment { get; set; }
        /// <summary>
        /// Кого оценили
        /// </summary>
        public UserDTO Owner { get; set; }
    }
}
