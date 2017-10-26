using Mite.CodeData.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mite.Models
{
    public abstract class BaseFilterModel
    {
        public int Page { get; set; }
        /// <summary>
        /// Когда был первый запрос(перед перезагрузкой страницы или сменой фильтра)
        /// </summary>
        public DateTime InitialDate { get; set; }
    }
    public class PostTopFilterModel : BaseFilterModel
    {
        /// <summary>
        /// Список тегов(через запятую)
        /// </summary>
        public string Tags { get; set; }
        /// <summary>
        /// Строка поста
        /// </summary>
        public string PostName { get; set; }
        public SortFilter SortFilter { get; set; }
        public PostTimeFilter PostTimeFilter { get; set; }
        public PostUserFilter PostUserFilter { get; set; }

        private string[] tagNames;
        public string[] TagNames
        {
            get
            {
                if (tagNames == null)
                    tagNames = string.IsNullOrEmpty(Tags) ? new string[0] : Tags.Split(',');
                return tagNames;
            }
        }
    }
    public class ServiceTopFilterModel : BaseFilterModel
    {
        public Guid? City { get; set; }
        public Guid? ServiceType { get; set; }
        public string Input { get; set; }
        public IEnumerable<SelectListItem> Cities { get; set; }
        public IEnumerable<SelectListItem> ServiceTypes { get; set; }
        public ServiceSortFilter SortFilter { get; set; }
        public int? Min { get; set; }
        public int? Max { get; set; }
    }
}