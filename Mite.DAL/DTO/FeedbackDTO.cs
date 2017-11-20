using Mite.DAL.Entities;

namespace Mite.DAL.DTO
{
    public class FeedbackDTO
    {
        public string Content { get; set; }
        public User User { get; set; }
    }
}
