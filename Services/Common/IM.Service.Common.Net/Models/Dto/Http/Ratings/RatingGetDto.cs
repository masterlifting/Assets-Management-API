using System;

namespace IM.Service.Common.Net.Models.Dto.Http.Ratings
{
    public record RatingGetDto
    {
        public string Company { get; init; } = null!;
        public int Place { get; init; }

        public decimal ResultPrice { get; init; }
        public decimal ResultReport { get; init; }
      
        public decimal Result { get; init; }

        public DateTime UpdateTime { get; init; }
    }
}
