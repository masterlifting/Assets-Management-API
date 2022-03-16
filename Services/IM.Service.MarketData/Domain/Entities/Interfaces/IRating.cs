using System.ComponentModel.DataAnnotations.Schema;
using IM.Service.MarketData.Domain.Entities.Catalogs;

namespace IM.Service.MarketData.Domain.Entities.Interfaces;

public interface IRating
{
    Status Status { get; set; }
    byte StatusId { get; set; }

    [Column(TypeName = "Decimal(18,4)")]
    decimal? Result { get; set; }
}