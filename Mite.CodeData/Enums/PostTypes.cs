using System.ComponentModel.DataAnnotations;

namespace Mite.CodeData.Enums
{
    public enum PostTypes
    {
        //По типу контента
        Image,
        Document,
        Article,
        /// <summary>
        /// Опубликованные
        /// </summary>
        Published,
        /// <summary>
        /// Неопубликованные(черновик)
        /// </summary>
        Drafts,
        /// <summary>
        /// Заблокированные
        /// </summary>
        Blocked
    }
}