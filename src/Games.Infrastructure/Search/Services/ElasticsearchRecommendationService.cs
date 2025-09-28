using Elasticsearch.Net;
using Games.Infrastructure.Search.Interfaces;
using Games.Infrastructure.Search.Models;
using Microsoft.Extensions.Logging;
using Nest;

namespace Games.Infrastructure.Search.Services
{
    public class ElasticsearchRecommendationService : IElasticsearchRecommendationService
    {
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<ElasticsearchRecommendationService> _logger;
        private const string IndexName = "games";

        public ElasticsearchRecommendationService(IElasticClient elasticClient, ILogger<ElasticsearchRecommendationService> logger)
        {
            _elasticClient = elasticClient;
            _logger = logger;
        }

        public async Task<GameRecommendationResult> GetGameBasedRecommendationsAsync(Guid gameId, int maxResults = 10, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting recommendations based on game {GameId}", gameId);

                // Primeiro, buscar o jogo base
                var baseGameResponse = await _elasticClient.GetAsync<GameDocument>(gameId.ToString(), g => g.Index(IndexName), cancellationToken);
                
                if (!baseGameResponse.IsValid || !baseGameResponse.Found)
                {
                    _logger.LogWarning("Base game {GameId} not found for recommendations", gameId);
                    return new GameRecommendationResult();
                }

                var baseGame = baseGameResponse.Source;
                var criteria = new List<string>();

                // Construir query de similaridade baseada no jogo
                var searchResponse = await _elasticClient.SearchAsync<GameDocument>(s => s
                    .Index(IndexName)
                    .Size(maxResults + 1) // +1 para excluir o próprio jogo
                    .Query(q => q
                        .Bool(b => b
                            .Must(m => m
                                .Bool(mb => mb
                                    .Should(
                                        // Similaridade por gêneros (peso alto)
                                        sh => sh.Terms(t => t.Field(f => f.Genres).Terms(baseGame.Genres).Boost(3.0)),
                                        // Similaridade por desenvolvedor (peso médio)
                                        sh => sh.Match(m => m.Field(f => f.Developer).Query(baseGame.Developer).Boost(2.0)),
                                        // Rating similar (peso médio)
                                        sh => sh.Range(r => r.Field(f => f.Rating)
                                            .GreaterThanOrEquals((double)(baseGame.Rating - 1.0m))
                                            .LessThanOrEquals((double)(baseGame.Rating + 1.0m))
                                            .Boost(1.5)),
                                        // Preço similar (peso baixo)
                                        sh => sh.Range(r => r.Field(f => f.Price)
                                            .GreaterThanOrEquals((double)(baseGame.Price * 0.7m))
                                            .LessThanOrEquals((double)(baseGame.Price * 1.3m))
                                            .Boost(1.0))
                                    )
                                    .MinimumShouldMatch(1)
                                )
                            )
                            .MustNot(mn => mn.Ids(i => i.Values(gameId.ToString()))) // Excluir o próprio jogo
                        )
                    ), cancellationToken);

                if (!searchResponse.IsValid)
                {
                    _logger.LogError("Error searching for similar games: {Error}", searchResponse.DebugInformation);
                    return new GameRecommendationResult();
                }

                criteria.AddRange(["Similar genres", "Similar rating range", "Developer match"]);

                var recommendedGames = searchResponse.Documents.Select(doc => new RecommendedGame
                {
                    Id = Guid.Parse(doc.Id.ToString()),
                    Title = doc.Title,
                    Description = doc.Description ?? string.Empty,
                    Price = doc.Price,
                    Rating = doc.Rating,
                    Developer = doc.Developer ?? string.Empty,
                    Genres = doc.Genres?.ToList() ?? new List<string>(),
                    ImageUrl = doc.ImageUrl ?? string.Empty,
                    SimilarityScore = CalculateSimilarityScore(baseGame, doc),
                    MatchingCriteria = GetMatchingCriteria(baseGame, doc)
                }).OrderByDescending(g => g.SimilarityScore).ToList();

                return new GameRecommendationResult
                {
                    RecommendedGames = recommendedGames,
                    BasedOnGameTitle = baseGame.Title,
                    BasedOnGameId = gameId,
                    RecommendationCriteria = criteria,
                    TotalRecommendations = recommendedGames.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting game-based recommendations for {GameId}", gameId);
                return new GameRecommendationResult();
            }
        }

        public async Task<SimilarGamesResult> FindSimilarGamesAsync(string searchQuery, List<string>? genres = null, decimal? minRating = null, int maxResults = 15, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Finding similar games for query: {Query}", searchQuery);

                var queryContainer = BuildSimilarGamesQuery(searchQuery, genres, minRating);
                var criteria = BuildSearchCriteria(searchQuery, genres, minRating);

                var searchResponse = await _elasticClient.SearchAsync<GameDocument>(s => s
                    .Index(IndexName)
                    .Size(maxResults)
                    .Query(q => queryContainer), cancellationToken);

                if (!searchResponse.IsValid)
                {
                    _logger.LogError("Error searching for similar games: {Error}", searchResponse.DebugInformation);
                    return new SimilarGamesResult { Query = searchQuery };
                }

                var similarGames = searchResponse.Documents.Select(doc => new SimilarGame
                {
                    Id = Guid.Parse(doc.Id.ToString()),
                    Title = doc.Title,
                    Price = doc.Price,
                    Rating = doc.Rating,
                    Developer = doc.Developer ?? string.Empty,
                    Genres = doc.Genres?.ToList() ?? new List<string>(),
                    SimilarityScore = CalculateQuerySimilarityScore(searchQuery, doc, genres),
                    ReasonForSimilarity = GetSimilarityReason(searchQuery, doc, genres)
                }).OrderByDescending(g => g.SimilarityScore).ToList();

                return new SimilarGamesResult
                {
                    SimilarGames = similarGames,
                    Query = searchQuery,
                    SearchCriteria = criteria,
                    TotalFound = similarGames.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding similar games for query: {Query}", searchQuery);
                return new SimilarGamesResult { Query = searchQuery };
            }
        }

        public async Task<GenreBasedRecommendationResult> GetGenreBasedRecommendationsAsync(List<string> preferredGenres, decimal? minRating = null, decimal? maxPrice = null, int maxResults = 5, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting genre-based recommendations for genres: {Genres}", string.Join(", ", preferredGenres));

                var result = new GenreBasedRecommendationResult
                {
                    InputGenres = preferredGenres,
                    RecommendationsByGenre = new List<GenreRecommendation>()
                };

                foreach (var genre in preferredGenres)
                {
                    var genreQuery = BuildGenreQuery(genre, minRating, maxPrice);
                    
                    var searchResponse = await _elasticClient.SearchAsync<GameDocument>(s => s
                        .Index(IndexName)
                        .Size(maxResults)
                        .Query(q => genreQuery)
                        .Sort(so => so.Descending(d => d.Rating)), cancellationToken);

                    if (searchResponse.IsValid && searchResponse.Documents.Any())
                    {
                        var games = searchResponse.Documents.Select(doc => new RecommendedGame
                        {
                            Id = Guid.Parse(doc.Id.ToString()),
                            Title = doc.Title,
                            Description = doc.Description ?? string.Empty,
                            Price = doc.Price,
                            Rating = doc.Rating,
                            Developer = doc.Developer ?? string.Empty,
                            Genres = doc.Genres?.ToList() ?? new List<string>(),
                            ImageUrl = doc.ImageUrl ?? string.Empty,
                            SimilarityScore = CalculateGenreSimilarityScore(doc, preferredGenres),
                            MatchingCriteria = [$"Genre: {genre}"]
                        }).ToList();

                        result.RecommendationsByGenre.Add(new GenreRecommendation
                        {
                            Genre = genre,
                            Games = games,
                            GenreWeight = CalculateGenreWeight(genre, preferredGenres)
                        });
                    }
                }

                result.TotalRecommendations = result.RecommendationsByGenre.Sum(r => r.Games.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting genre-based recommendations for genres: {Genres}", string.Join(", ", preferredGenres));
                return new GenreBasedRecommendationResult { InputGenres = preferredGenres };
            }
        }

        public async Task<UserPreferencesRecommendationResult> GetPersonalizedRecommendationsAsync(UserPreferencesProfile userProfile, int maxResults = 20, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting personalized recommendations for user profile");

                var strategies = new List<string>();
                var personalizedQuery = BuildPersonalizedQuery(userProfile, strategies);

                var searchResponse = await _elasticClient.SearchAsync<GameDocument>(s => s
                    .Index(IndexName)
                    .Size(maxResults)
                    .Query(q => personalizedQuery)
                    .Sort(so => so.Descending(SortSpecialField.Score).Descending(d => d.Rating)), cancellationToken);

                if (!searchResponse.IsValid)
                {
                    _logger.LogError("Error getting personalized recommendations: {Error}", searchResponse.DebugInformation);
                    return new UserPreferencesRecommendationResult { UserProfile = userProfile };
                }

                var personalizedGames = searchResponse.Documents.Select(doc => new RecommendedGame
                {
                    Id = Guid.Parse(doc.Id.ToString()),
                    Title = doc.Title,
                    Description = doc.Description ?? string.Empty,
                    Price = doc.Price,
                    Rating = doc.Rating,
                    Developer = doc.Developer ?? string.Empty,
                    Genres = doc.Genres?.ToList() ?? new List<string>(),
                    ImageUrl = doc.ImageUrl ?? string.Empty,
                    SimilarityScore = CalculatePersonalizedScore(doc, userProfile),
                    MatchingCriteria = GetPersonalizedCriteria(doc, userProfile)
                }).OrderByDescending(g => g.SimilarityScore).ToList();

                return new UserPreferencesRecommendationResult
                {
                    PersonalizedGames = personalizedGames,
                    UserProfile = userProfile,
                    RecommendationStrategies = strategies,
                    TotalRecommendations = personalizedGames.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting personalized recommendations");
                return new UserPreferencesRecommendationResult { UserProfile = userProfile };
            }
        }

        public async Task<SimilarGamesResult> GetDeveloperBasedRecommendationsAsync(string developer, List<Guid>? excludeGameIds = null, int maxResults = 10, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting developer-based recommendations for: {Developer}", developer);

                var developerQuery = new BoolQuery
                {
                    Should = new QueryContainer[]
                    {
                        // Exact match com desenvolvedor (peso alto)
                        new MatchQuery { Field = "developer", Query = developer, Boost = 3.0 },
                        // Fuzzy match com desenvolvedor (peso médio)
                        new FuzzyQuery { Field = "developer", Value = developer, Boost = 2.0 }
                    },
                    MustNot = excludeGameIds?.Select(id => (QueryContainer)new IdsQuery { Values = new Id[] { id.ToString() } }) ?? new QueryContainer[0]
                };

                var searchResponse = await _elasticClient.SearchAsync<GameDocument>(s => s
                    .Index(IndexName)
                    .Size(maxResults)
                    .Query(q => developerQuery)
                    .Sort(so => so.Descending(SortSpecialField.Score).Descending(d => d.Rating)), cancellationToken);

                if (!searchResponse.IsValid)
                {
                    _logger.LogError("Error getting developer-based recommendations: {Error}", searchResponse.DebugInformation);
                    return new SimilarGamesResult();
                }

                var similarGames = searchResponse.Documents.Select(doc => new SimilarGame
                {
                    Id = Guid.Parse(doc.Id.ToString()),
                    Title = doc.Title,
                    Price = doc.Price,
                    Rating = doc.Rating,
                    Developer = doc.Developer ?? string.Empty,
                    Genres = doc.Genres?.ToList() ?? new List<string>(),
                    SimilarityScore = CalculateDeveloperSimilarityScore(doc.Developer, developer),
                    ReasonForSimilarity = GetDeveloperSimilarityReason(doc.Developer, developer)
                }).OrderByDescending(g => g.SimilarityScore).ToList();

                return new SimilarGamesResult
                {
                    SimilarGames = similarGames,
                    Query = developer,
                    SearchCriteria = ["Developer similarity", "Rating quality"],
                    TotalFound = similarGames.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting developer-based recommendations for: {Developer}", developer);
                return new SimilarGamesResult();
            }
        }

        #region Private Helper Methods

        private QueryContainer BuildSimilarGamesQuery(string searchQuery, List<string>? genres, decimal? minRating)
        {
            var mustQueries = new List<QueryContainer>
            {
                new MultiMatchQuery
                {
                    Query = searchQuery,
                    Fields = new[] { "title^2.0", "description^1.0", "developer^1.5" },
                    Fuzziness = Fuzziness.Auto
                }
            };

            if (genres != null && genres.Any())
            {
                mustQueries.Add(new TermsQuery
                {
                    Field = "genres",
                    Terms = genres
                });
            }

            if (minRating.HasValue)
            {
                mustQueries.Add(new NumericRangeQuery
                {
                    Field = "rating",
                    GreaterThanOrEqualTo = (double)minRating.Value
                });
            }

            return new BoolQuery
            {
                Must = mustQueries
            };
        }

        private QueryContainer BuildGenreQuery(string genre, decimal? minRating, decimal? maxPrice)
        {
            var mustQueries = new List<QueryContainer>
            {
                new TermQuery
                {
                    Field = "genres",
                    Value = genre
                }
            };

            if (minRating.HasValue)
            {
                mustQueries.Add(new NumericRangeQuery
                {
                    Field = "rating",
                    GreaterThanOrEqualTo = (double)minRating.Value
                });
            }

            if (maxPrice.HasValue)
            {
                mustQueries.Add(new NumericRangeQuery
                {
                    Field = "price",
                    LessThanOrEqualTo = (double)maxPrice.Value
                });
            }

            return new BoolQuery
            {
                Must = mustQueries
            };
        }

        private QueryContainer BuildPersonalizedQuery(UserPreferencesProfile userProfile, List<string> strategies)
        {
            var shouldQueries = new List<QueryContainer>();

            // Gêneros preferidos (peso alto)
            if (userProfile.PreferredGenres.Any())
            {
                shouldQueries.Add(new TermsQuery
                {
                    Field = "genres",
                    Terms = userProfile.PreferredGenres,
                    Boost = 3.0
                });
                strategies.Add("Preferred genres matching");
            }

            // Desenvolvedores preferidos (peso médio-alto)
            if (userProfile.PreferredDevelopers.Any())
            {
                shouldQueries.Add(new TermsQuery
                {
                    Field = "developer",
                    Terms = userProfile.PreferredDevelopers,
                    Boost = 2.5
                });
                strategies.Add("Preferred developers matching");
            }

            // Rating mínimo
            if (userProfile.MinRating > 0)
            {
                strategies.Add("Minimum rating filter");
            }

            // Preferência de preço
            if (userProfile.PricePreference != "any")
            {
                strategies.Add($"Price preference: {userProfile.PricePreference}");
            }

            var mustQueries = new List<QueryContainer>();

            if (userProfile.MinRating > 0)
            {
                mustQueries.Add(new NumericRangeQuery
                {
                    Field = "rating",
                    GreaterThanOrEqualTo = (double)userProfile.MinRating
                });
            }

            if (userProfile.MaxPrice > 0)
            {
                mustQueries.Add(new NumericRangeQuery
                {
                    Field = "price",
                    LessThanOrEqualTo = (double)userProfile.MaxPrice
                });
            }

            var priceQuery = GetPricePreferenceQuery(userProfile.PricePreference);
            if (priceQuery != null)
            {
                mustQueries.Add(priceQuery);
            }

            return new BoolQuery
            {
                Should = shouldQueries,
                Must = mustQueries,
                MinimumShouldMatch = shouldQueries.Any() ? 1 : 0
            };
        }

        private QueryContainer GetPricePreferenceQuery(string pricePreference)
        {
            return pricePreference switch
            {
                "free" => new TermQuery { Field = "price", Value = 0 },
                "budget" => new NumericRangeQuery { Field = "price", GreaterThan = 0, LessThanOrEqualTo = 50 },
                "premium" => new NumericRangeQuery { Field = "price", GreaterThan = 100 },
                _ => null
            };
        }

        private double CalculateSimilarityScore(GameDocument baseGame, GameDocument compareGame)
        {
            double score = 0.0;

            // Similaridade de gêneros (40% do peso)
            var genreIntersection = baseGame.Genres?.Intersect(compareGame.Genres ?? new List<string>()).Count() ?? 0;
            var genreUnion = baseGame.Genres?.Union(compareGame.Genres ?? new List<string>()).Count() ?? 1;
            score += (genreIntersection / (double)genreUnion) * 0.4;

            // Similaridade de rating (25% do peso)
            var ratingDiff = Math.Abs((double)(baseGame.Rating - compareGame.Rating));
            score += (1.0 - Math.Min(ratingDiff / 10.0, 1.0)) * 0.25;

            // Similaridade de desenvolvedor (20% do peso)
            if (baseGame.Developer == compareGame.Developer)
                score += 0.2;

            // Similaridade de preço (15% do peso)
            var priceDiff = Math.Abs((double)(baseGame.Price - compareGame.Price));
            var maxPrice = Math.Max((double)baseGame.Price, (double)compareGame.Price);
            if (maxPrice > 0)
                score += (1.0 - Math.Min(priceDiff / maxPrice, 1.0)) * 0.15;
            else
                score += 0.15; // Ambos gratuitos

            return Math.Round(score * 100, 2);
        }

        private List<string> GetMatchingCriteria(GameDocument baseGame, GameDocument compareGame)
        {
            var criteria = new List<string>();

            var sharedGenres = baseGame.Genres?.Intersect(compareGame.Genres ?? new List<string>()).ToList() ?? new List<string>();
            if (sharedGenres.Any())
                criteria.Add($"Shared genres: {string.Join(", ", sharedGenres)}");

            if (baseGame.Developer == compareGame.Developer)
                criteria.Add($"Same developer: {baseGame.Developer}");

            var ratingDiff = Math.Abs((double)(baseGame.Rating - compareGame.Rating));
            if (ratingDiff <= 1.0)
                criteria.Add("Similar rating");

            return criteria;
        }

        private double CalculateQuerySimilarityScore(string searchQuery, GameDocument doc, List<string>? genres)
        {
            double score = 0.0;

            // Score baseado na relevância do texto (60%)
            if (doc.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                score += 0.4;
            if (doc.Description?.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) == true)
                score += 0.2;

            // Score baseado nos gêneros (40%)
            if (genres != null && genres.Any())
            {
                var matchingGenres = doc.Genres?.Intersect(genres).Count() ?? 0;
                score += (matchingGenres / (double)genres.Count) * 0.4;
            }

            return Math.Round(score * 100, 2);
        }

        private string GetSimilarityReason(string searchQuery, GameDocument doc, List<string>? genres)
        {
            var reasons = new List<string>();

            if (doc.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                reasons.Add("Title match");
            if (doc.Description?.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) == true)
                reasons.Add("Description match");

            if (genres != null)
            {
                var matchingGenres = doc.Genres?.Intersect(genres).ToList() ?? new List<string>();
                if (matchingGenres.Any())
                    reasons.Add($"Genre match: {string.Join(", ", matchingGenres)}");
            }

            return reasons.Any() ? string.Join("; ", reasons) : "General similarity";
        }

        private List<string> BuildSearchCriteria(string searchQuery, List<string>? genres, decimal? minRating)
        {
            var criteria = new List<string> { $"Query: {searchQuery}" };
            
            if (genres != null && genres.Any())
                criteria.Add($"Genres: {string.Join(", ", genres)}");
            
            if (minRating.HasValue)
                criteria.Add($"Min Rating: {minRating.Value}");

            return criteria;
        }

        private double CalculateGenreSimilarityScore(GameDocument doc, List<string> preferredGenres)
        {
            var matchingGenres = doc.Genres?.Intersect(preferredGenres).Count() ?? 0;
            var genreScore = matchingGenres / (double)preferredGenres.Count;
            
            // Combinar com rating (70% gênero, 30% rating)
            var ratingScore = (double)doc.Rating / 10.0;
            
            return Math.Round((genreScore * 0.7 + ratingScore * 0.3) * 100, 2);
        }

        private double CalculateGenreWeight(string genre, List<string> preferredGenres)
        {
            // Primeiro gênero tem peso maior
            var index = preferredGenres.IndexOf(genre);
            return index == 0 ? 1.0 : 1.0 - (index * 0.1);
        }

        private double CalculatePersonalizedScore(GameDocument doc, UserPreferencesProfile userProfile)
        {
            double score = 0.0;

            // Score por gêneros preferidos (40%)
            if (userProfile.PreferredGenres.Any())
            {
                var matchingGenres = doc.Genres?.Intersect(userProfile.PreferredGenres).Count() ?? 0;
                score += (matchingGenres / (double)userProfile.PreferredGenres.Count) * 0.4;
            }

            // Score por desenvolvedor preferido (25%)
            if (userProfile.PreferredDevelopers.Contains(doc.Developer))
                score += 0.25;

            // Score por rating (20%)
            var ratingScore = (double)doc.Rating / 10.0;
            score += ratingScore * 0.2;

            // Score por preferência de preço (15%)
            score += CalculatePricePreferenceScore(doc.Price, userProfile) * 0.15;

            return Math.Round(score * 100, 2);
        }

        private double CalculatePricePreferenceScore(decimal gamePrice, UserPreferencesProfile userProfile)
        {
            return userProfile.PricePreference switch
            {
                "free" => gamePrice == 0 ? 1.0 : 0.0,
                "budget" => gamePrice > 0 && gamePrice <= 50 ? 1.0 : 0.5,
                "premium" => gamePrice > 100 ? 1.0 : 0.7,
                _ => 1.0 // any
            };
        }

        private List<string> GetPersonalizedCriteria(GameDocument doc, UserPreferencesProfile userProfile)
        {
            var criteria = new List<string>();

            var matchingGenres = doc.Genres?.Intersect(userProfile.PreferredGenres).ToList() ?? new List<string>();
            if (matchingGenres.Any())
                criteria.Add($"Preferred genres: {string.Join(", ", matchingGenres)}");

            if (userProfile.PreferredDevelopers.Contains(doc.Developer))
                criteria.Add($"Preferred developer: {doc.Developer}");

            if (doc.Rating >= userProfile.MinRating)
                criteria.Add($"Meets rating requirement: {doc.Rating}");

            return criteria;
        }

        private double CalculateDeveloperSimilarityScore(string? gamesDeveloper, string targetDeveloper)
        {
            if (string.IsNullOrEmpty(gamesDeveloper)) return 0.0;

            if (gamesDeveloper.Equals(targetDeveloper, StringComparison.OrdinalIgnoreCase))
                return 100.0;

            // Similaridade aproximada usando distância de Levenshtein simplificada
            var similarity = 1.0 - (LevenshteinDistance(gamesDeveloper, targetDeveloper) / (double)Math.Max(gamesDeveloper.Length, targetDeveloper.Length));
            return Math.Round(similarity * 75, 2); // Max 75% para matches não exatos
        }

        private string GetDeveloperSimilarityReason(string? gamesDeveloper, string targetDeveloper)
        {
            if (string.IsNullOrEmpty(gamesDeveloper)) return "Unknown developer";

            if (gamesDeveloper.Equals(targetDeveloper, StringComparison.OrdinalIgnoreCase))
                return $"Exact developer match: {targetDeveloper}";

            return $"Similar developer: {gamesDeveloper}";
        }

        private int LevenshteinDistance(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1)) return string.IsNullOrEmpty(s2) ? 0 : s2.Length;
            if (string.IsNullOrEmpty(s2)) return s1.Length;

            var matrix = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++) matrix[i, 0] = i;
            for (int j = 0; j <= s2.Length; j++) matrix[0, j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                    matrix[i, j] = Math.Min(Math.Min(
                        matrix[i - 1, j] + 1,
                        matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[s1.Length, s2.Length];
        }

        #endregion
    }
}