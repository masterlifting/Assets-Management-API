namespace IM.Service.Shared.Models.RabbitMq.Api;

public record RatingMqDto(string AssetId, byte AssetTypeId, int Place);