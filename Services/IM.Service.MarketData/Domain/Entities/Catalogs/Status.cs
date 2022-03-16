using IM.Service.Common.Net.Models.Entity;

namespace IM.Service.MarketData.Domain.Entities.Catalogs;

public class Status : Catalog
{
    public IEnumerable<Report>? Reports { get; set; }
    public IEnumerable<Price>? Prices { get; set; }
    public IEnumerable<Coefficient>? Coefficients { get; set; }
    public IEnumerable<Dividend>? Dividends { get; set; }
}