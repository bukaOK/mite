using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.CodeData.Enums
{
    /// <summary>
    /// Типы продолжительности
    /// </summary>
    public enum DurationTypes : byte
    {
        [Display(Name = "Час")]
        Hour = 0,
        [Display(Name = "День")]
        Day = 1,
        [Display(Name = "Неделя")]
        Week = 2,
        [Display(Name = "Месяц")]
        Month = 3
    }
}
