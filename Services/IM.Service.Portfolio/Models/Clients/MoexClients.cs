namespace IM.Service.Portfolio.Models.Clients;

public record MoexIsinData(Securities Securities);
public record Securities(object[][] Data);