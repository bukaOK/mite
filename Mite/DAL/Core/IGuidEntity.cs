using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mite.DAL.Core
{
    public interface IGuidEntity
    {
        Guid Id { get; set; }
    }
}