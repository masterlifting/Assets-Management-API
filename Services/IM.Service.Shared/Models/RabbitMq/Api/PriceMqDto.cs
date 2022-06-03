namespace IM.Service.Shared.Models.RabbitMq.Api;

public record PriceMqDto(string CompanyId, decimal PriceLast, decimal PriceAvg);