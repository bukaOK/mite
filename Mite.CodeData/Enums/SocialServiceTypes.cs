using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Mite.CodeData.Enums
{
    public enum SocialServiceTypes
    {
        [Display(Name = "ВКонтакте")]
        Vk = 0,
        [Display(Name = "Facebook")]
        Facebook = 1,
        [Display(Name = "ArtStation")]
        ArtStation = 2,
        [Display(Name = "Твиттер")]
        Twitter = 3,
        [Display(Name = "Dribbble")]
        Dribbble = 4,
        [Display(Name = "Проза.ру")]
        ProzaRu = 5,
        [Display(Name = "Стихи.ру")]
        StihiRu = 6,
        [Display(Name = "Instagram")]
        Instagram = 7
    }
}