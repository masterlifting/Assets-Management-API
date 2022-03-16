namespace IM.Service.MarketData.Models.Api.Http;

public record SourceGetDto(string Name, string? Value);
public record SourcePostDto(byte Id, string? Value);