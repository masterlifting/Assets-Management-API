using IM.Services.Companies.Prices.Api.DataAccess.Entities;

namespace IM.Services.Companies.Prices.Api.Models.Dto
{
    public class PriceDto : CommonServices.Models.Dto.PriceDto
    {
        public PriceDto(Price price, string sourceType, string ticker)
        {
            TickerName = ticker.ToUpperInvariant();
            SourceType = sourceType;
            if (price is not null)
            {
                Date = price.Date;
                Value = price.Value;
            }
        }
    }
}