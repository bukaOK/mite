using System.ComponentModel.DataAnnotations;

namespace Mite.CodeData.Enums
{
    public enum CharacterOriginalType : byte
    {
        [Display(Name = "Любой")]
        All = 0,
        [Display(Name = "Оригинальный")]
        Original = 1,
        [Display(Name = "Известный")]
        NonOriginal = 2
    }
}
