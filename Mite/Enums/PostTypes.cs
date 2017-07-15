using System.ComponentModel.DataAnnotations;

namespace Mite.Enums
{
    public enum PostTypes
    {
        //По типу контента
        Image,
        Document,
        Article,
        //Опубликованные / неопубликованные(черновик)
        Published,
        Drafts,
        /// <summary>
        /// Заблокированные
        /// </summary>
        Blocked
    }
}