using System.Threading.Tasks;

using IM.Service.Shared.Models.RabbitMq.Api;

namespace IM.Service.Recommendations.Services.Entity;

public class PurchaseService
{
    public Task SetAsync(PriceMqDto price)
    {
        throw new System.NotImplementedException();
    }

    public Task DeleteAsync(PriceMqDto price)
    {
        throw new System.NotImplementedException();
    }
    public Task SetAsync(PriceMqDto[] prices)
    {
        throw new System.NotImplementedException();
    }

    public Task DeleteAsync(PriceMqDto[] prices)
    {
        throw new System.NotImplementedException();
    }
    public Task SetAsync(RatingMqDto[] ratings)
    {
        throw new System.NotImplementedException();
    }

    public Task DeleteAsync(RatingMqDto[] ratings)
    {
        throw new System.NotImplementedException();
    }
}