namespace IM.Service.Broker.Data.Models.Dto.Mq;

public record ReportFileDto(string Name, string ContentType, byte[] Payload, string UserId);
