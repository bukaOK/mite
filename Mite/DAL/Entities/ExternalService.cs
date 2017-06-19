using Microsoft.AspNet.Identity.EntityFramework;
using Mite.DAL.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mite.DAL.Entities
{
    /// <summary>
    /// Сущность внешность сервисов(google plus, vk, facebook etc.)
    /// </summary>
    public class ExternalService : Entity
    {
        public string Name { get; set; }
        public string AccessToken { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }
    }
}