using IM.Services.Company.Prices.DataAccess.Entities;

namespace IM.Services.Company.Prices.Models.Dto
{
    public class PriceDto : CommonServices.Models.Dto.PriceDto
    {
        public PriceDto(Price price, byte sourceTypeId, string sourceType)
        {
            SourceTypeId = sourceTypeId;
            SourceType = sourceType;

            if (price is not null)
            {
                TickerName = price.TickerName;
                Date = price.Date;
                Value = price.Value;
            }
        }
    }
}