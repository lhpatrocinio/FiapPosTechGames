using System;
using System.Collections.Generic;

namespace Games.Domain.Entities
{
    public class Game
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal Rating { get; set; }
        public string Developer { get; set; }
        public string IndicatedAgeRating { get; set; }
        public decimal HourPlayed { get; set; }
        public string ImageUrl { get; set; }

        public ICollection<GameGenre> Genres { get; set; }
    }
}