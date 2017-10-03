using System.ComponentModel.DataAnnotations;

namespace Mite.CodeData.Enums
{
    /// <summary>
    /// По скрытности(обычно отображается на странице пользователя)
    /// </summary>
    public enum PostTypes : byte
    {
        //По скрытности
        /// <summary>
        /// Опубликованные
        /// </summary>
        Published = 3,
        /// <summary>
        /// Неопубликованные(черновик)
        /// </summary>
        Drafts = 4,
        /// <summary>
        /// Заблокированные
        /// </summary>
        Blocked = 5,
        /// <summary>
        /// Избранное
        /// </summary>
        Favorite = 7,
        /// <summary>
        /// Не заполненные после импорта
        /// </summary>
        NotFilled = 8
    }
}