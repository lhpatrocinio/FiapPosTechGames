using Games.Infrastructure.Search.Interfaces;
using Games.Infrastructure.Search.Models;
using Microsoft.Extensions.Logging;
using Nest;

namespace Games.Infrastructure.Search.Services
{
    public class ElasticsearchAnalyticsService : IElasticsearchAnalyticsService
    {
        private readonly IElasticClient _client;
        private readonly ILogger<ElasticsearchAnalyticsService> _logger;
        private const string IndexName = "games";

        public ElasticsearchAnalyticsService(IElasticClient client, ILogger<ElasticsearchAnalyticsService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<PopularGamesResult> GetPopularGamesAsync(int limit = 10)
        {
            try
            {
                var searchResponse = await _client.SearchAsync<GameDocument>(s => s
                    .Index(IndexName)
                    .Size(limit)
                    .Query(q => q.MatchAll())
                    .Sort(sort => sort
                        .Descending(f => f.Rating)
                        .Descending(f => f.HourPlayed)
                    )
                );

                if (searchResponse.IsValid)
                {
                    var games = searchResponse.Documents.Select(doc => new PopularGameItem
                    {
                        Title = doc.Title,
                        SearchCount = (long)doc.HourPlayed, // Proxy para popularidade
                        AverageRating = (double)doc.Rating,
                        Price = doc.Price,
                        Genres = doc.Genres
                    }).ToList();

                    return new PopularGamesResult
                    {
                        Games = games,
                        TotalGames = searchResponse.Total
                    };
                }

                _logger.LogError("Failed to get popular games: {Error}", searchResponse.OriginalException?.Message);
                return new PopularGamesResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular games");
                return new PopularGamesResult();
            }
        }

        public async Task<GenreStatsResult> GetGenreStatisticsAsync()
        {
            try
            {
                var searchResponse = await _client.SearchAsync<GameDocument>(s => s
                    .Index(IndexName)
                    .Size(0) // Não precisamos dos documentos, só das agregações
                    .Aggregations(a => a
                        .Terms("genres_stats", t => t
                            .Field(f => f.Genres)
                            .Size(20)
                            .Aggregations(aa => aa
                                .Average("avg_price", avg => avg.Field(f => f.Price))
                                .Average("avg_rating", avg => avg.Field(f => f.Rating))
                                .Max("max_price", max => max.Field(f => f.Price))
                                .Min("min_price", min => min.Field(f => f.Price))
                                .ValueCount("games_count", vc => vc.Field(f => f.Id))
                            )
                        )
                    )
                );

                if (searchResponse.IsValid && searchResponse.Aggregations.ContainsKey("genres_stats"))
                {
                    var genresAgg = searchResponse.Aggregations.Terms("genres_stats");
                    var genreStats = genresAgg.Buckets.Select(bucket => new GenreStatItem
                    {
                        Genre = bucket.Key,
                        GamesCount = bucket.DocCount ?? 0L,
                        AveragePrice = bucket.Average("avg_price")?.Value ?? 0,
                        AverageRating = bucket.Average("avg_rating")?.Value ?? 0,
                        HighestPrice = (decimal)(bucket.Max("max_price")?.Value ?? 0),
                        LowestPrice = (decimal)(bucket.Min("min_price")?.Value ?? 0)
                    }).ToList();

                    return new GenreStatsResult
                    {
                        GenreStats = genreStats,
                        TotalGenres = genreStats.Count
                    };
                }

                _logger.LogError("Failed to get genre statistics: {Error}", searchResponse.OriginalException?.Message);
                return new GenreStatsResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting genre statistics");
                return new GenreStatsResult();
            }
        }

        public async Task<PriceRangeStatsResult> GetPriceRangeAnalyticsAsync()
        {
            try
            {
                var searchResponse = await _client.SearchAsync<GameDocument>(s => s
                    .Index(IndexName)
                    .Size(0)
                    .Aggregations(a => a
                        .Range("price_ranges", r => r
                            .Field(f => f.Price)
                            .Ranges(
                                ranges => ranges.To(0.01), // Grátis
                                ranges => ranges.From(0.01).To(50), // Baixo
                                ranges => ranges.From(50).To(150), // Médio
                                ranges => ranges.From(150).To(300), // Alto
                                ranges => ranges.From(300) // Premium
                            )
                            .Aggregations(aa => aa
                                .TopHits("sample_games", th => th
                                    .Size(3)
                                    .Source(src => src.Includes(i => i.Field(f => f.Title)))
                                )
                            )
                        )
                        .Average("overall_avg_price", avg => avg.Field(f => f.Price))
                    )
                );

                if (searchResponse.IsValid && searchResponse.Aggregations.ContainsKey("price_ranges"))
                {
                    var priceRangesAgg = searchResponse.Aggregations.Range("price_ranges");
                    var totalGames = priceRangesAgg.Buckets.Sum(b => b.DocCount);
                    
                    var priceRanges = priceRangesAgg.Buckets.Select((bucket, index) => 
                    {
                        var rangeName = index switch
                        {
                            0 => "Grátis (R$ 0,00)",
                            1 => "Baixo (R$ 0,01 - R$ 50,00)",
                            2 => "Médio (R$ 50,01 - R$ 150,00)",
                            3 => "Alto (R$ 150,01 - R$ 300,00)",
                            4 => "Premium (R$ 300,01+)",
                            _ => "Outro"
                        };

                        var sampleTitles = new List<string>();
                        if (bucket.TopHits("sample_games")?.Documents<GameDocument>() != null)
                        {
                            sampleTitles = bucket.TopHits("sample_games")
                                .Documents<GameDocument>()
                                .Select(d => d.Title)
                                .ToList();
                        }

                        return new PriceRangeItem
                        {
                            Range = rangeName,
                            Count = bucket.DocCount,
                            Percentage = totalGames > 0 ? Math.Round((double)bucket.DocCount / totalGames * 100, 2) : 0,
                            SampleTitles = sampleTitles
                        };
                    }).ToList();

                    return new PriceRangeStatsResult
                    {
                        PriceRanges = priceRanges,
                        OverallAveragePrice = searchResponse.Aggregations.Average("overall_avg_price")?.Value ?? 0,
                        TotalGames = totalGames
                    };
                }

                _logger.LogError("Failed to get price range analytics: {Error}", searchResponse.OriginalException?.Message);
                return new PriceRangeStatsResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting price range analytics");
                return new PriceRangeStatsResult();
            }
        }

        public async Task<TopRatedGamesResult> GetTopRatedGamesAsync(int limit = 10)
        {
            try
            {
                var searchResponse = await _client.SearchAsync<GameDocument>(s => s
                    .Index(IndexName)
                    .Size(limit)
                    .Query(q => q.MatchAll())
                    .Sort(sort => sort.Descending(f => f.Rating))
                    .Aggregations(a => a
                        .Average("overall_avg_rating", avg => avg.Field(f => f.Rating))
                    )
                );

                if (searchResponse.IsValid)
                {
                    var games = searchResponse.Documents.Select(doc => new TopRatedGameItem
                    {
                        Id = doc.Id,
                        Title = doc.Title,
                        Rating = doc.Rating,
                        Price = doc.Price,
                        Developer = doc.Developer,
                        Genres = doc.Genres
                    }).ToList();

                    return new TopRatedGamesResult
                    {
                        Games = games,
                        OverallAverageRating = searchResponse.Aggregations.Average("overall_avg_rating")?.Value ?? 0
                    };
                }

                _logger.LogError("Failed to get top rated games: {Error}", searchResponse.OriginalException?.Message);
                return new TopRatedGamesResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top rated games");
                return new TopRatedGamesResult();
            }
        }

        public async Task<CatalogOverviewResult> GetCatalogOverviewAsync()
        {
            try
            {
                var searchResponse = await _client.SearchAsync<GameDocument>(s => s
                    .Index(IndexName)
                    .Size(0)
                    .Aggregations(a => a
                        .ValueCount("total_games", vc => vc.Field(f => f.Id))
                        .Cardinality("unique_genres", c => c.Field(f => f.Genres))
                        .Average("avg_price", avg => avg.Field(f => f.Price))
                        .Average("avg_rating", avg => avg.Field(f => f.Rating))
                        .Filter("free_games", f => f
                            .Filter(ff => ff.Range(r => r.Field(field => field.Price).LessThanOrEquals(0)))
                        )
                        .Filter("paid_games", f => f
                            .Filter(ff => ff.Range(r => r.Field(field => field.Price).GreaterThan(0)))
                        )
                        .Terms("popular_genre", t => t
                            .Field(f => f.Genres)
                            .Size(1)
                        )
                        .Max("highest_price", max => max.Field(f => f.Price))
                        .Min("lowest_price", min => min
                            .Field(f => f.Price)
                            .Missing(0) // Considerar jogos grátis
                        )
                    )
                );

                if (searchResponse.IsValid)
                {
                    var popularGenre = "";
                    if (searchResponse.Aggregations.Terms("popular_genre")?.Buckets?.FirstOrDefault() != null)
                    {
                        popularGenre = searchResponse.Aggregations.Terms("popular_genre").Buckets.First().Key;
                    }

                    return new CatalogOverviewResult
                    {
                        TotalGames = (long)(searchResponse.Aggregations.ValueCount("total_games")?.Value ?? 0),
                        TotalGenres = (long)(searchResponse.Aggregations.Cardinality("unique_genres")?.Value ?? 0),
                        AveragePrice = searchResponse.Aggregations.Average("avg_price")?.Value ?? 0,
                        AverageRating = searchResponse.Aggregations.Average("avg_rating")?.Value ?? 0,
                        FreeGamesCount = searchResponse.Aggregations.Filter("free_games")?.DocCount ?? 0L,
                        PaidGamesCount = searchResponse.Aggregations.Filter("paid_games")?.DocCount ?? 0L,
                        MostPopularGenre = popularGenre,
                        HighestPricedGame = (decimal)(searchResponse.Aggregations.Max("highest_price")?.Value ?? 0),
                        LowestPricedGame = (decimal)(searchResponse.Aggregations.Min("lowest_price")?.Value ?? 0)
                    };
                }

                _logger.LogError("Failed to get catalog overview: {Error}", searchResponse.OriginalException?.Message);
                return new CatalogOverviewResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting catalog overview");
                return new CatalogOverviewResult();
            }
        }
    }
}