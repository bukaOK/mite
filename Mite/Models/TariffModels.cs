using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Mite.Models
{
    public class TariffModel
    {
        public Guid? Id { get; set; }
        [Required]
        [MaxLength(200)]
        [UIHint("TextBox")]
        [DisplayName("Название тарифа")]
        public string Header { get; set; }
        [Required]
        [AllowHtml]
        public string Description { get; set; }
        [Required]
        [UIHint("TextBox")]
        [DisplayName("Ежемесячный платеж")]
        public int? Price { get; set; }
        public string AuthorId { get; set; }
        public UserShortModel Author { get; set; }
    }
    /// <summary>
    /// Модель списка тарифов для клиента
    /// </summary>
    public class ClientTariffsModel
    {
        public UserShortModel Author { get; set; }
        public TariffModel SelectedTariff { get; set; }
        public IList<TariffModel> Tariffs { get; set; }
    }
}