namespace IM.Service.Shared.Models.RabbitMq.Api;

public record DealMqDto(string AssetId, byte AssetTypeId, decimal? SumValue, decimal? SumCost, decimal? CostLast);