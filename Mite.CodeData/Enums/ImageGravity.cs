using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.CodeData.Enums
{
    /// <summary>
    /// Image watermark positions
    /// </summary>
    public enum ImageGravity : byte
    {
        [Display(Name = "Сверху слева")]
        NorthWest = 1,
        [Display(Name = "Сверху справа")]
        NorthEast = 3,
        [Display(Name = "Сверху")]
        North = 2,
        [Display(Name = "Снизу слева")]
        SouthWest = 7,
        [Display(Name = "Снизу справа")]
        SouthEast = 9,
        [Display(Name = "Снизу")]
        South = 8,
        [Display(Name = "По центру")]
        Center = 5,
        [Display(Name = "Справа")]
        East = 6,
        [Display(Name = "Слева")]
        West = 4
    }
}
