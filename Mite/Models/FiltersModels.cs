using Mite.CodeData.Enums;
using System;
using System.Collections.Generic;
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
        /// ПоискЫ
        /// </summary>
        public string Input { get; set; }
        public TopSort Sort { get; set; }
        public bool OnlyFollowings { get; set; }

        //private string[] tagNames;
        //public string[] TagNames
        //{
        //    get
        //    {
        //        if (tagNames == null)
        //            tagNames = string.IsNullOrEmpty(Tags) ? new string[0] : Tags.Split(',');
        //        return tagNames;
        //    }
        //}
    }
    public class ServiceTopFilterModel : BaseFilterModel
    {
        public Guid? City { get; set; }
        public Guid? ServiceType { get; set; }
        public string Input { get; set; }
        public IEnumerable<SelectListItem> Countries { get; set; }
        public IEnumerable<SelectListItem> Cities { get; set; }
        public IEnumerable<SelectListItem> ServiceTypes { get; set; }
        public ServiceSortFilter SortFilter { get; set; }
        public int? Min { get; set; }
        public int? Max { get; set; }
    }
    public class OrderTopFilterModel : BaseFilterModel
    {
        public Guid? OrderTypeId { get; set; }
        public IEnumerable<SelectListItem> OrderTypes { get; set; }
        public Guid? CityId { get; set; }
        public IEnumerable<SelectListItem> Countries { get; set; }
        public IEnumerable<SelectListItem> Cities { get; set; }
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }
        public string Input { get; set; }
    }
    public class ProductTopFilterModel : BaseFilterModel
    {
        public string Input { get; set; }
        /// <summary>
        /// Искать товары только для авторов
        /// </summary>
        public bool ForAuthors { get; set; }
        public Guid? City { get; set; }
        public IEnumerable<SelectListItem> Cities { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }
        public string CurrentUserId { get; set; }
    }
    public class CharacterTopFilterModel
    {
        public CharacterFilter Type { get; set; }
        public CharacterOriginalType OriginalType { get; set; }
        public string Input { get; set; }
    }
}