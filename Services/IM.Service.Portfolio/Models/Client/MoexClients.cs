namespace IM.Service.Portfolio.Models.Client;

public record MoexIsinData(Securities Securities);
public record Securities(object[][] Data);