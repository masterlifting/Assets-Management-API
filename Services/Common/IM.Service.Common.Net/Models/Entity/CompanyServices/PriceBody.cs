using System.ComponentModel.DataAnnotations.Schema;
using IM.Service.Common.Net.Attributes;

namespace IM.Service.Common.Net.Models.Entity.CompanyServices;

public abstract class PriceBody : SourceTypeBody
{
    [NotZero(nameof(Value)), Column(TypeName = "Decimal(18,4)")]
    public decimal Value { get; set; }
}