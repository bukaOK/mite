using System.ComponentModel.DataAnnotations;

namespace Mite.Enums
{
    /// <summary>
    /// Тип пользователя, определяется в первую очередь рейтингом разных постов
    /// если рейтинг у двух типов одинаковый, учитываем количество
    /// </summary>
    public enum MemberTypes : byte
    {
        [Display(Name = "Художник")]
        Artist = 0,
        [Display(Name = "Писатель")]
        Writer = 1,
        [Display(Name = "Фотограф")]
        Photographer = 2
    }
}