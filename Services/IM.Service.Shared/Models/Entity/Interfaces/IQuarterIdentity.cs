namespace IM.Service.Shared.Models.Entity.Interfaces;

public interface IQuarterIdentity : IPeriod
{
    int Year { get; set; }
    byte Quarter { get; set; }
}