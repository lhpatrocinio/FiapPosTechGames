namespace Games.Infrastructure.Search.Models
{
    public class PopularGamesResult
    {
        public List<PopularGameItem> Games { get; set; } = new();
        public long TotalGames { get; set; }
    }

    public class PopularGameItem
    {
        public string Title { get; set; } = string.Empty;
        public long SearchCount { get; set; }
        public double AverageRating { get; set; }
        public decimal Price { get; set; }
        public List<string> Genres { get; set; } = new();
    }

    public class GenreStatsResult
    {
        public List<GenreStatItem> GenreStats { get; set; } = new();
        public long TotalGenres { get; set; }
    }

    public class GenreStatItem
    {
        public string Genre { get; set; } = string.Empty;
        public long GamesCount { get; set; }
        public double AveragePrice { get; set; }
        public double AverageRating { get; set; }
        public decimal HighestPrice { get; set; }
        public decimal LowestPrice { get; set; }
    }

    public class PriceRangeStatsResult
    {
        public List<PriceRangeItem> PriceRanges { get; set; } = new();
        public double OverallAveragePrice { get; set; }
        public long TotalGames { get; set; }
    }

    public class PriceRangeItem
    {
        public string Range { get; set; } = string.Empty;
        public long Count { get; set; }
        public double Percentage { get; set; }
        public List<string> SampleTitles { get; set; } = new();
    }

    public class TopRatedGamesResult
    {
        public List<TopRatedGameItem> Games { get; set; } = new();
        public double OverallAverageRating { get; set; }
    }

    public class TopRatedGameItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public decimal Price { get; set; }
        public string Developer { get; set; } = string.Empty;
        public List<string> Genres { get; set; } = new();
    }
}