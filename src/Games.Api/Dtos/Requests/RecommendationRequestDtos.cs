namespace Games.Api.Dtos.Requests
{
    public class SimilarGamesRequestDto
    {
        public string SearchQuery { get; set; } = string.Empty;
        public List<string>? Genres { get; set; }
        public decimal? MinRating { get; set; }
        public int MaxResults { get; set; } = 15;
    }

    public class GenreBasedRecommendationRequestDto
    {
        public List<string> PreferredGenres { get; set; } = new();
        public decimal? MinRating { get; set; }
        public decimal? MaxPrice { get; set; }
        public int MaxResults { get; set; } = 5;
    }

    public class PersonalizedRecommendationRequestDto
    {
        public List<string> PreferredGenres { get; set; } = new();
        public decimal MinRating { get; set; } = 0;
        public decimal MaxPrice { get; set; } = 0;
        public List<string> PreferredDevelopers { get; set; } = new();
        public string PricePreference { get; set; } = "any"; // "free", "budget", "premium", "any"
        public int MaxResults { get; set; } = 20;
    }

    public class DeveloperBasedRecommendationRequestDto
    {
        public string Developer { get; set; } = string.Empty;
        public List<Guid>? ExcludeGameIds { get; set; }
        public int MaxResults { get; set; } = 10;
    }
}