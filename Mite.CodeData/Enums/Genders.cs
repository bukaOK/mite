using System.ComponentModel.DataAnnotations;

namespace Mite.CodeData.Enums
{
    public enum Genders : byte
    {
        [Display(Name = "Мужской")]
        Male,
        [Display(Name = "Женский")]
        Female
    }
}