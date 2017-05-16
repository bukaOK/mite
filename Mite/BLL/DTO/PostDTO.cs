using System;
using System.Collections.Generic;
using Mite.Models;

namespace Mite.BLL.DTO
{
    public class PostDTO
    {
        public Guid Id { get; set; }
        public string Header { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public byte PostType { get; set; }
        public List<TagDTO> Tags { get; set; }
        public List<CommentModel> Comments { get; set; }
    }
}