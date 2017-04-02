using System.ComponentModel.DataAnnotations;

namespace Mite.Enums
{
    public enum Genders : byte
    {
        [Display(Name = "Мужской")]
        Male,
        [Display(Name = "Женский")]
        Female
    }
}