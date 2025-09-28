using Games.Domain.Entities;

namespace Games.Infrastructure.Search.Models
{
    public class GameDocument
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Rating { get; set; }
        public string Developer { get; set; } = string.Empty;
        public string IndicatedAgeRating { get; set; } = string.Empty;
        public decimal HourPlayed { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public List<string> Genres { get; set; } = new();
        public DateTime IndexedAt { get; set; }

        public static GameDocument FromGame(Game game)
        {
            return new GameDocument
            {
                Id = game.Id,
                Title = game.Title,
                Description = game.Description,
                Price = game.Price,
                Rating = game.Rating,
                Developer = game.Developer,
                IndicatedAgeRating = game.IndicatedAgeRating,
                HourPlayed = game.HourPlayed,
                ImageUrl = game.ImageUrl,
                Genres = game.Genres?.Select(g => g.GenreType.Description).ToList() ?? new List<string>(),
                IndexedAt = DateTime.UtcNow
            };
        }
    }
}
