using Elasticsearch.Net;
using Games.Infrastructure.Search.Interfaces;
using Games.Infrastructure.Search.Models;
using Microsoft.Extensions.Logging;
using Nest;

namespace Games.Infrastructure.Search.Services
{
    public class ElasticsearchService : IElasticsearchService
    {
        private readonly IElasticClient _client;
        private readonly ILogger<ElasticsearchService> _logger;
        private const string IndexName = "games";

        public ElasticsearchService(IElasticClient client, ILogger<ElasticsearchService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<bool> EnsureIndexExistsAsync()
        {
            try
            {
                var existsResponse = await _client.Indices.ExistsAsync(IndexName);
                if (existsResponse.Exists)
                {
                    _logger.LogInformation("Elasticsearch index '{IndexName}' already exists", IndexName);
                    return true;
                }

                var createIndexResponse = await _client.Indices.CreateAsync(IndexName, c => c
                    .Map<GameDocument>(m => m
                        .Properties(p => p
                            .Keyword(k => k.Name(n => n.Id))
                            .Text(t => t.Name(n => n.Title).Analyzer("standard"))
                            .Text(t => t.Name(n => n.Description).Analyzer("standard"))
                            .Number(n => n.Name(f => f.Price).Type(NumberType.ScaledFloat).ScalingFactor(100))
                            .Number(n => n.Name(f => f.Rating).Type(NumberType.HalfFloat))
                            .Text(t => t.Name(n => n.Developer).Analyzer("standard"))
                            .Keyword(k => k.Name(n => n.IndicatedAgeRating))
                            .Number(n => n.Name(f => f.HourPlayed).Type(NumberType.Float))
                            .Keyword(k => k.Name(n => n.ImageUrl))
                            .Keyword(k => k.Name(n => n.Genres))
                            .Date(d => d.Name(n => n.IndexedAt))
                        )
                    )
                );

                if (createIndexResponse.IsValid)
                {
                    _logger.LogInformation("Elasticsearch index '{IndexName}' created successfully", IndexName);
                    return true;
                }

                _logger.LogError("Failed to create Elasticsearch index: {Error}", createIndexResponse.OriginalException?.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring Elasticsearch index exists");
                return false;
            }
        }

        public async Task<bool> IndexGameAsync(GameDocument game)
        {
            try
            {
                var response = await _client.IndexDocumentAsync(game);
                
                if (response.IsValid)
                {
                    _logger.LogDebug("Game '{GameTitle}' indexed successfully", game.Title);
                    return true;
                }

                _logger.LogError("Failed to index game '{GameTitle}': {Error}", game.Title, response.OriginalException?.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing game '{GameTitle}'", game.Title);
                return false;
            }
        }

        public async Task<bool> IndexGamesAsync(IEnumerable<GameDocument> games)
        {
            try
            {
                var response = await _client.IndexManyAsync(games, IndexName);
                
                if (response.IsValid)
                {
                    _logger.LogInformation("Bulk indexed {Count} games successfully", games.Count());
                    return true;
                }

                _logger.LogError("Failed to bulk index games: {Error}", response.OriginalException?.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk indexing games");
                return false;
            }
        }

        public async Task<bool> UpdateGameAsync(GameDocument game)
        {
            try
            {
                var response = await _client.UpdateAsync<GameDocument>(game.Id, u => u
                    .Index(IndexName)
                    .Doc(game)
                    .DocAsUpsert(true)
                );

                if (response.IsValid)
                {
                    _logger.LogDebug("Game '{GameTitle}' updated successfully", game.Title);
                    return true;
                }

                _logger.LogError("Failed to update game '{GameTitle}': {Error}", game.Title, response.OriginalException?.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating game '{GameTitle}'", game.Title);
                return false;
            }
        }

        public async Task<bool> DeleteGameAsync(Guid gameId)
        {
            try
            {
                var response = await _client.DeleteAsync<GameDocument>(gameId, d => d.Index(IndexName));

                if (response.IsValid)
                {
                    _logger.LogDebug("Game with ID '{GameId}' deleted successfully", gameId);
                    return true;
                }

                _logger.LogError("Failed to delete game '{GameId}': {Error}", gameId, response.OriginalException?.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting game '{GameId}'", gameId);
                return false;
            }
        }

        public async Task<SearchResult<GameDocument>> SearchGamesAsync(GameSearchRequest request)
        {
            try
            {
                var searchResponse = await _client.SearchAsync<GameDocument>(s => s
                    .Index(IndexName)
                    .From(request.From)
                    .Size(request.Size)
                    .Query(q => BuildQuery(q, request))
                );

                if (searchResponse.IsValid)
                {
                    return new SearchResult<GameDocument>
                    {
                        Documents = searchResponse.Documents.ToList(),
                        Total = searchResponse.Total,
                        From = request.From,
                        Size = request.Size
                    };
                }

                _logger.LogError("Search failed: {Error}", searchResponse.OriginalException?.Message);
                return new SearchResult<GameDocument>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching games");
                return new SearchResult<GameDocument>();
            }
        }

        private QueryContainer BuildQuery(QueryContainerDescriptor<GameDocument> q, GameSearchRequest request)
        {
            var queries = new List<QueryContainer>();

            if (!string.IsNullOrWhiteSpace(request.Query))
            {
                queries.Add(q.MultiMatch(mm => mm
                    .Query(request.Query)
                    .Fields(f => f
                        .Field(fd => fd.Title, boost: 2.0)
                        .Field(fd => fd.Description)
                        .Field(fd => fd.Developer)
                    )
                    .Fuzziness(Fuzziness.Auto)
                ));
            }

            if (request.Genres?.Any() == true)
            {
                queries.Add(q.Terms(t => t.Field(f => f.Genres).Terms(request.Genres)));
            }

            if (request.MinPrice.HasValue || request.MaxPrice.HasValue)
            {
                queries.Add(q.Range(r => r
                    .Field(f => f.Price)
                    .GreaterThanOrEquals((double?)request.MinPrice)
                    .LessThanOrEquals((double?)request.MaxPrice)
                ));
            }

            if (request.MinRating.HasValue)
            {
                queries.Add(q.Range(r => r
                    .Field(f => f.Rating)
                    .GreaterThanOrEquals((double?)request.MinRating)
                ));
            }

            if (!string.IsNullOrWhiteSpace(request.Developer))
            {
                queries.Add(q.Match(m => m
                    .Field(f => f.Developer)
                    .Query(request.Developer)
                ));
            }

            return queries.Any() 
                ? q.Bool(b => b.Must(queries.ToArray()))
                : q.MatchAll();
        }
    }
}
