using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mite.BLL.DTO
{
    /// <summary>
    /// Сообщество пользователя
    /// </summary>
    public class GroupDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public int Rating { get; set; }
        public List<UserDTO> Members { get; set; }
        public byte MemberType { get; set; }
    }
}