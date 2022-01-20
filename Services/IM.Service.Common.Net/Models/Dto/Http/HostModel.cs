namespace IM.Service.Common.Net.Models.Dto.Http;

public class HostModel
{
    public string Schema { get; set; } = null!;
    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public string? ApiKey { get; set; }
}