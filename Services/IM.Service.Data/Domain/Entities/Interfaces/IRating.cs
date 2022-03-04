using IM.Service.Data.Domain.Entities.Catalogs;

using System.ComponentModel.DataAnnotations.Schema;

namespace IM.Service.Data.Domain.Entities.Interfaces;

public interface IRating
{
    Status Status { get; set; }
    byte StatusId { get; set; }

    [Column(TypeName = "Decimal(18,4)")]
    decimal? Result { get; set; }
}