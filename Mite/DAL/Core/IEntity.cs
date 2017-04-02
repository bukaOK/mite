using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mite.DAL.Core
{
    public interface IEntity
    {
        Guid Id { get; set; }
    }
}