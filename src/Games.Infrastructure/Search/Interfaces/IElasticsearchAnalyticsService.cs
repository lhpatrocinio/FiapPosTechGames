using Games.Infrastructure.Search.Models;

namespace Games.Infrastructure.Search.Interfaces
{
    public interface IElasticsearchAnalyticsService
    {
        /// <summary>
        /// Obtém os jogos mais populares baseado em rating e horas jogadas
        /// </summary>
        Task<PopularGamesResult> GetPopularGamesAsync(int limit = 10);

        /// <summary>
        /// Obtém estatísticas agregadas por gênero
        /// </summary>
        Task<GenreStatsResult> GetGenreStatisticsAsync();

        /// <summary>
        /// Obtém análise de distribuição de preços
        /// </summary>
        Task<PriceRangeStatsResult> GetPriceRangeAnalyticsAsync();

        /// <summary>
        /// Obtém os jogos com melhor rating
        /// </summary>
        Task<TopRatedGamesResult> GetTopRatedGamesAsync(int limit = 10);

        /// <summary>
        /// Obtém estatísticas gerais do catálogo
        /// </summary>
        Task<CatalogOverviewResult> GetCatalogOverviewAsync();
    }

    public class CatalogOverviewResult
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