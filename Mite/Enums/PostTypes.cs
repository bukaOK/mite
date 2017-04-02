using System.ComponentModel.DataAnnotations;

namespace Mite.Enums
{
    public enum PostTypes
    {
        //По типу контента
        Image,
        Document,
        //Опубликованные / неопубликованные(черновик)
        Published,
        Drafts
    }
}