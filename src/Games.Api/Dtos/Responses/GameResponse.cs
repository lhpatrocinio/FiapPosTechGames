using System.Collections.Generic;
using System;

namespace Core.PosTech8Nett.Api.Domain.Model.Game
{
    public class GameResponse
    {
        public GameResponse()
        {
            Genres = new List<string>();
        }

        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal Rating { get; set; }
        public string Developer { get; set; }
        public string IndicatedAgeRating { get; set; }
        public decimal HourPlayed { get; set; }
        public string ImageUrl { get; set; }
        public List<string> Genres { get; set; }
    }
}
