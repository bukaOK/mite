using Mite.CodeData.Enums;

namespace Mite.DAL.Filters
{
    public class CharacterTopFilter
    {
        public string UserId { get; set; }
        public CharacterFilter Filter { get; set; }
        public CharacterOriginalType OriginalType { get; set; }
        public string Input { get; set; }
    }
}
