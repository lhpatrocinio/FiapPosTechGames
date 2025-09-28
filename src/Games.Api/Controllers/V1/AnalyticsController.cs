using Asp.Versioning;
using AutoMapper;
using Games.Api.Dtos.Responses;
using Games.Infrastructure.Search.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Games.Api.Controllers.V1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize(Roles = "Games")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IElasticsearchAnalyticsService _analyticsService;
        private readonly IMapper _mapper;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(
            IElasticsearchAnalyticsService analyticsService,
            IMapper mapper,
            ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Obtém os jogos mais populares baseado em rating e horas jogadas
        /// </summary>
        /// <param name="limit">Número máximo de jogos a retornar (padrão: 10)</param>
        /// <returns>Lista dos jogos mais populares</returns>
        [HttpGet("popular-games")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(AnalyticsPopularGamesResponse))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        public async Task<IActionResult> GetPopularGames([FromQuery] int limit = 10)
        {
            try
            {
                if (limit <= 0 || limit > 50)
                {
                    return BadRequest("Limit deve estar entre 1 e 50");
                }

                var result = await _analyticsService.GetPopularGamesAsync(limit);
                var response = _mapper.Map<AnalyticsPopularGamesResponse>(result);

                _logger.LogInformation("Popular games analytics executed: {Count} games returned", result.Games.Count);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular games analytics");
                return StatusCode(503, "Erro interno ao obter jogos populares");
            }
        }

        /// <summary>
        /// Obtém estatísticas agregadas por gênero
        /// </summary>
        /// <returns>Estatísticas detalhadas por gênero</returns>
        [HttpGet("genres-stats")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(AnalyticsGenreStatsResponse))]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        public async Task<IActionResult> GetGenreStatistics()
        {
            try
            {
                var result = await _analyticsService.GetGenreStatisticsAsync();
                var response = _mapper.Map<AnalyticsGenreStatsResponse>(result);

                _logger.LogInformation("Genre statistics analytics executed: {Count} genres analyzed", result.GenreStats.Count);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting genre statistics");
                return StatusCode(503, "Erro interno ao obter estatísticas de gênero");
            }
        }

        /// <summary>
        /// Obtém análise de distribuição de preços
        /// </summary>
        /// <returns>Distribuição de jogos por faixas de preço</returns>
        [HttpGet("price-analytics")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(AnalyticsPriceRangeResponse))]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        public async Task<IActionResult> GetPriceRangeAnalytics()
        {
            try
            {
                var result = await _analyticsService.GetPriceRangeAnalyticsAsync();
                var response = _mapper.Map<AnalyticsPriceRangeResponse>(result);

                _logger.LogInformation("Price range analytics executed: {TotalGames} games analyzed across {RangeCount} price ranges", 
                    result.TotalGames, result.PriceRanges.Count);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting price range analytics");
                return StatusCode(503, "Erro interno ao obter análise de preços");
            }
        }

        /// <summary>
        /// Obtém os jogos com melhor rating
        /// </summary>
        /// <param name="limit">Número máximo de jogos a retornar (padrão: 10)</param>
        /// <returns>Lista dos jogos com melhor avaliação</returns>
        [HttpGet("top-rated")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(AnalyticsTopRatedResponse))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        public async Task<IActionResult> GetTopRatedGames([FromQuery] int limit = 10)
        {
            try
            {
                if (limit <= 0 || limit > 50)
                {
                    return BadRequest("Limit deve estar entre 1 e 50");
                }

                var result = await _analyticsService.GetTopRatedGamesAsync(limit);
                var response = _mapper.Map<AnalyticsTopRatedResponse>(result);

                _logger.LogInformation("Top rated games analytics executed: {Count} games returned, overall average rating: {AvgRating}", 
                    result.Games.Count, result.OverallAverageRating);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top rated games");
                return StatusCode(503, "Erro interno ao obter jogos mais bem avaliados");
            }
        }

        /// <summary>
        /// Obtém visão geral completa do catálogo
        /// </summary>
        /// <returns>Estatísticas gerais do catálogo de jogos</returns>
        [HttpGet("catalog-overview")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(AnalyticsCatalogOverviewResponse))]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        public async Task<IActionResult> GetCatalogOverview()
        {
            try
            {
                var result = await _analyticsService.GetCatalogOverviewAsync();
                var response = _mapper.Map<AnalyticsCatalogOverviewResponse>(result);

                _logger.LogInformation("Catalog overview analytics executed: {TotalGames} total games, {TotalGenres} genres, most popular genre: {PopularGenre}", 
                    result.TotalGames, result.TotalGenres, result.MostPopularGenre);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting catalog overview");
                return StatusCode(503, "Erro interno ao obter visão geral do catálogo");
            }
        }
    }
}