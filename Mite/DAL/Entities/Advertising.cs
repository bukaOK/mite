using Dapper.Contrib.Extensions;
using Mite.DAL.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mite.DAL.Entities
{
    public class Advertising : IGuidEntity
    {
        [Key]
        public Guid Id { get; set; }
        public IList<User> ClickedUsers { get; set; }
        [ForeignKey("CashOperation")]
        public string CashOperationId { get; set; }
        public CashOperation CashOperation { get; set; }
    }
}