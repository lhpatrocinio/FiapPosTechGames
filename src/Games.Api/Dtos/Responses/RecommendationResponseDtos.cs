namespace Games.Api.Dtos.Responses
{
    public class GameRecommendationResponseDto
    {
        public List<RecommendedGameDto> RecommendedGames { get; set; } = new();
        public string BasedOnGameTitle { get; set; } = string.Empty;
        public Guid BasedOnGameId { get; set; }
        public List<string> RecommendationCriteria { get; set; } = new();
        public int TotalRecommendations { get; set; }
    }

    public class RecommendedGameDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Rating { get; set; }
        public string Developer { get; set; } = string.Empty;
        public List<string> Genres { get; set; } = new();
        public string ImageUrl { get; set; } = string.Empty;
        public double SimilarityScore { get; set; }
        public List<string> MatchingCriteria { get; set; } = new();
    }

    public class SimilarGamesResponseDto
    {
        public List<SimilarGameDto> SimilarGames { get; set; } = new();
        public string Query { get; set; } = string.Empty;
        public List<string> SearchCriteria { get; set; } = new();
        public int TotalFound { get; set; }
    }

    public class SimilarGameDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Rating { get; set; }
        public string Developer { get; set; } = string.Empty;
        public List<string> Genres { get; set; } = new();
        public double SimilarityScore { get; set; }
        public string ReasonForSimilarity { get; set; } = string.Empty;
    }

    public class GenreBasedRecommendationResponseDto
    {
        public List<GenreRecommendationDto> RecommendationsByGenre { get; set; } = new();
        public List<string> InputGenres { get; set; } = new();
        public int TotalRecommendations { get; set; }
    }

    public class GenreRecommendationDto
    {
        public string Genre { get; set; } = string.Empty;
        public List<RecommendedGameDto> Games { get; set; } = new();
        public double GenreWeight { get; set; }
    }

    public class UserPreferencesRecommendationResponseDto
    {
        public List<RecommendedGameDto> PersonalizedGames { get; set; } = new();
        public UserPreferencesProfileDto UserProfile { get; set; } = new();
        public List<string> RecommendationStrategies { get; set; } = new();
        public int TotalRecommendations { get; set; }
    }

    public class UserPreferencesProfileDto
    {
        public List<string> PreferredGenres { get; set; } = new();
        public decimal MinRating { get; set; }
        public decimal MaxPrice { get; set; }
        public List<string> PreferredDevelopers { get; set; } = new();
        public string PricePreference { get; set; } = string.Empty;
    }
}