namespace IM.Service.Portfolio.Models.Api.Mq;

public record ReportFileDto(string Name, string ContentType, byte[] Payload, string UserId);
