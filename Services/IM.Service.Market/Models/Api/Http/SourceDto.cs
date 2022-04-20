namespace IM.Service.Market.Models.Api.Http;

public record SourceGetDto(byte Id, string Name, string? Value);
public record SourcePostDto(byte Id, string? Value);