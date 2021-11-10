using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IM.Service.Common.Net.Models.Entity.Interfaces
{
    public interface IDateIdentity
    {
        [Column(TypeName = "Date")]
        DateTime Date { get; init; }
    }
}
