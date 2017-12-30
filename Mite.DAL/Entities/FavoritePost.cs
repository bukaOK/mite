using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.DAL.Entities
{
    public class FavoritePost
    {
        public User User { get; set; }
        [Key, ForeignKey("User"), Column(Order = 0)]
        public string UserId { get; set; }
        public Post Post { get; set; }
        [Key, ForeignKey("Post"), Column(Order = 1)]
        public Guid PostId { get; set; }
    }
}
