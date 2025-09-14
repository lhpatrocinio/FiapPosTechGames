using System.Collections.Generic;
using System;

namespace Core.PosTech8Nett.Api.Domain.Model.Game
{
    public class GameRequest
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal Rating { get; set; }
        public string Developer { get; set; } = null!;
        public string IndicatedAgeRating { get; set; } = null!;
        public decimal HourPlayed { get; set; }
        public string ImageUrl { get; set; } = null!;
    }
}
