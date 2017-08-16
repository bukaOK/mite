using Mite.DAL.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mite.BLL.DTO
{
    public class AdvertisingDTO
    {
        public Guid Id { get; set; }
        public IList<UserDTO> ClickedUsers { get; set; }
        public string CashOperationId { get; set; }
        public CashOperationDTO CashOperation { get; set; }
    }
}