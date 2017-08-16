using Microsoft.AspNet.Identity.EntityFramework;
using Mite.DAL.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mite.BLL.DTO
{
    /// <summary>
    /// Сущность внешних сервисов(google plus, vk, facebook etc.)
    /// </summary>
    public class ExternalServiceDTO
    {
        public string Name { get; set; }
        /// <summary>
        /// Токен для внешнего сервиса(для GoogleApi это может быть RefreshToken)
        /// </summary>
        public string AccessToken { get; set; }
        public string UserId { get; set; }
        public UserDTO User { get; set; }
    }
}