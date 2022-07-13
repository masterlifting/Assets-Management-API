namespace IM.Service.Portfolio.Models.Api.Mq;

public record ProviderReportDto(string Name, string ContentType, byte[] Payload, string UserId);
