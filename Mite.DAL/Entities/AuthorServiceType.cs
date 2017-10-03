using Mite.DAL.Core;
using System.ComponentModel.DataAnnotations;

namespace Mite.DAL.Entities
{
    public class AuthorServiceType : GuidEntity
    {
        [MaxLength(200)]
        public string Name { get; set; }
        public bool Confirmed { get; set; }
    }
}