
using Mite.BLL.Core;
using System.Collections.Generic;

namespace Mite.BLL.DTO
{
    public class PostGroupDTO : GuidDTO
    {
        public string Name { get; set; }
        public IEnumerable<PostDTO> Posts { get; set; }
    }
}