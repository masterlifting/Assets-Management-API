using IM.Service.Common.Net.Models.Dto.Mq.CompanyServices;

namespace IM.Service.Company.Models.Dto
{
    public record CompanyGetDto
    {
        public string Ticker { get; init; } = null!;
        public string Name { get; init; } = null!;
        public string Industry { get; init; } = null!;
        public string Sector { get; init; } = null!;
        public string? Description { get; init; }
        public EntityTypeDto[]? DataSources { get; init; }
        public EntityTypeDto[]? Brokers { get; init; }
    }
}
