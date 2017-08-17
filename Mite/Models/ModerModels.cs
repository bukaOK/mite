using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Mite.Models
{
    public class CityModel
    {
        public Guid Id { get; set; }
        [DisplayName("Название")]
        [UIHint("TextBox")]
        public string Name { get; set; }
        [DisplayName("Регион")]
        [UIHint("TextBox")]
        public string Region { get; set; }
        [DisplayName("Федеральный округ")]
        [UIHint("TextBox")]
        public string District { get; set; }
        [DisplayName("Часовой пояс")]
        [UIHint("TextBox")]
        public int? TimeZone { get; set; }
        [DisplayName("Население")]
        [UIHint("TextBox")]
        public int Population { get; set; }
    }
}