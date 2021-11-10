namespace IM.Service.Common.Net.Models.Entity.Interfaces
{
    public interface IQuarterIdentity
    {
        int Year { get; init; }
        byte Quarter { get; init; }
    }
}
