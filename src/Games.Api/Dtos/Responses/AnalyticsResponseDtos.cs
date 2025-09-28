namespace Games.Api.Dtos.Responses
{
    public class AnalyticsPopularGamesResponse
    {
        public List<PopularGameDto> Games { get; set; } = new();
        public long TotalGames { get; set; }
    }

    public class PopularGameDto
    {
        public string Title { get; set; } = string.Empty;
        public long SearchCount { get; set; }
        public double AverageRating { get; set; }
        public decimal Price { get; set; }
        public List<string> Genres { get; set; } = new();
    }

    public class AnalyticsGenreStatsResponse
    {
        public List<GenreStatsDto> GenreStats { get; set; } = new();
        public long TotalGenres { get; set; }
    }

    public class GenreStatsDto
    {
        public string Genre { get; set; } = string.Empty;
        public long GamesCount { get; set; }
        public double AveragePrice { get; set; }
        public double AverageRating { get; set; }
        public decimal HighestPrice { get; set; }
        public decimal LowestPrice { get; set; }
    }

    public class AnalyticsPriceRangeResponse
    {
        public List<PriceRangeDto> PriceRanges { get; set; } = new();
        public double OverallAveragePrice { get; set; }
        public long TotalGames { get; set; }
    }

    public class PriceRangeDto
    {
        public string Range { get; set; } = string.Empty;
        public long Count { get; set; }
        public double Percentage { get; set; }
        public List<string> SampleTitles { get; set; } = new();
    }

    public class AnalyticsTopRatedResponse
    {
        public List<TopRatedGameDto> Games { get; set; } = new();
        public double OverallAverageRating { get; set; }
    }

    public class TopRatedGameDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public decimal Price { get; set; }
        public string Developer { get; set; } = string.Empty;
        public List<string> Genres { get; set; } = new();
    }

    public class AnalyticsCatalogOverviewResponse
    {
        public long TotalGames { get; set; }
        public long TotalGenres { get; set; }
        public double AveragePrice { get; set; }
        public double AverageRating { get; set; }
        public long FreeGamesCount { get; set; }
        public long PaidGamesCount { get; set; }
        public string MostPopularGenre { get; set; } = string.Empty;
        public decimal HighestPricedGame { get; set; }
        public decimal LowestPricedGame { get; set; }
    }
}