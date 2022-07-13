namespace IM.Service.Shared.Models.RabbitMq.Api;

public record CostMqDto(string AssetId, byte AssetTypeId, decimal CostFact, decimal CostAvg);