using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Mite.DAL.Entities
{
    [Table("TagPosts")]
    public class TagPost
    {
        [Required, Column(Order = 0)]
        [Key, ForeignKey("Tag")]
        public Guid Tag_Id { get; set; }
        [Required, Column(Order = 1)]
        [Key, ForeignKey("Post")]
        public Guid Post_Id { get; set; }

        public Tag Tag { get; set; }
        public Post Post { get; set; }
    }
}