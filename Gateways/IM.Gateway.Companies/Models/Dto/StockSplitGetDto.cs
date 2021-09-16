
using CommonServices.Models.Dto;

namespace IM.Gateway.Companies.Models.Dto
{
    public class StockSplitGetDto : StockSplitDto
    {
        public string Company { get; set; } = null!;
    }
}
