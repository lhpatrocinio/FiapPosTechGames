using Asp.Versioning;
using AutoMapper;
using Games.Api.Dtos.Requests;
using Games.Api.Dtos.Responses;
using Games.Infrastructure.Search.Interfaces;
using Games.Infrastructure.Search.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Games.Api.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(Roles = "Games")]
    public class RecommendationController : ControllerBase
    {
        private readonly IElasticsearchRecommendationService _recommendationService;
        private readonly IMapper _mapper;
        private readonly ILogger<RecommendationController> _logger;

        public RecommendationController(
            IElasticsearchRecommendationService recommendationService,
            IMapper mapper,
            ILogger<RecommendationController> logger)
        {
            _recommendationService = recommendationService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet("game-based/{gameId}")]
        [ProducesResponseType(typeof(GameRecommendationResponseDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetGameBasedRecommendations(Guid gameId, [FromQuery] int maxResults = 10)
        {
            try
            {
                if (gameId == Guid.Empty)
                {
                    _logger.LogWarning("Invalid gameId provided for recommendations");
                    return BadRequest("GameId must be a valid GUID");
                }

                _logger.LogInformation("Getting recommendations based on game {GameId}", gameId);

                var result = await _recommendationService.GetGameBasedRecommendationsAsync(gameId, maxResults);
                
                if (!result.RecommendedGames.Any())
                {
                    _logger.LogInformation("No recommendations found for game {GameId}", gameId);
                    return NotFound($"No recommendations found for game {gameId}");
                }

                var response = _mapper.Map<GameRecommendationResponseDto>(result);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting game-based recommendations for {GameId}", gameId);
                return StatusCode((int)HttpStatusCode.InternalServerError, "Error processing recommendation request");
            }
        }

        [HttpPost("similar-games")]
        [ProducesResponseType(typeof(SimilarGamesResponseDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> FindSimilarGames([FromBody] SimilarGamesRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.SearchQuery))
                {
                    _logger.LogWarning("Empty search query provided for similar games");
                    return BadRequest("SearchQuery is required");
                }

                _logger.LogInformation("Finding similar games for query: {Query}", request.SearchQuery);

                var result = await _recommendationService.FindSimilarGamesAsync(
                    request.SearchQuery, 
                    request.Genres, 
                    request.MinRating, 
                    request.MaxResults);

                var response = _mapper.Map<SimilarGamesResponseDto>(result);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding similar games for query: {Query}", request.SearchQuery);
                return StatusCode((int)HttpStatusCode.InternalServerError, "Error processing similar games request");
            }
        }

        [HttpPost("genre-based")]
        [ProducesResponseType(typeof(GenreBasedRecommendationResponseDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetGenreBasedRecommendations([FromBody] GenreBasedRecommendationRequestDto request)
        {
            try
            {
                if (!request.PreferredGenres.Any())
                {
                    _logger.LogWarning("No preferred genres provided for recommendations");
                    return BadRequest("At least one preferred genre is required");
                }

                _logger.LogInformation("Getting genre-based recommendations for: {Genres}", string.Join(", ", request.PreferredGenres));

                var result = await _recommendationService.GetGenreBasedRecommendationsAsync(
                    request.PreferredGenres,
                    request.MinRating,
                    request.MaxPrice,
                    request.MaxResults);

                var response = _mapper.Map<GenreBasedRecommendationResponseDto>(result);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting genre-based recommendations for genres: {Genres}", string.Join(", ", request.PreferredGenres));
                return StatusCode((int)HttpStatusCode.InternalServerError, "Error processing genre-based recommendations");
            }
        }

        [HttpPost("personalized")]
        [ProducesResponseType(typeof(UserPreferencesRecommendationResponseDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPersonalizedRecommendations([FromBody] PersonalizedRecommendationRequestDto request)
        {
            try
            {
                _logger.LogInformation("Getting personalized recommendations");

                var userProfile = new UserPreferencesProfile
                {
                    PreferredGenres = request.PreferredGenres,
                    MinRating = request.MinRating,
                    MaxPrice = request.MaxPrice,
                    PreferredDevelopers = request.PreferredDevelopers,
                    PricePreference = request.PricePreference
                };

                var result = await _recommendationService.GetPersonalizedRecommendationsAsync(userProfile, request.MaxResults);
                var response = _mapper.Map<UserPreferencesRecommendationResponseDto>(result);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting personalized recommendations");
                return StatusCode((int)HttpStatusCode.InternalServerError, "Error processing personalized recommendations");
            }
        }

        [HttpPost("developer-based")]
        [ProducesResponseType(typeof(SimilarGamesResponseDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetDeveloperBasedRecommendations([FromBody] DeveloperBasedRecommendationRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Developer))
                {
                    _logger.LogWarning("Empty developer name provided for recommendations");
                    return BadRequest("Developer name is required");
                }

                _logger.LogInformation("Getting developer-based recommendations for: {Developer}", request.Developer);

                var result = await _recommendationService.GetDeveloperBasedRecommendationsAsync(
                    request.Developer,
                    request.ExcludeGameIds,
                    request.MaxResults);

                var response = _mapper.Map<SimilarGamesResponseDto>(result);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting developer-based recommendations for: {Developer}", request.Developer);
                return StatusCode((int)HttpStatusCode.InternalServerError, "Error processing developer-based recommendations");
            }
        }

        [HttpGet("health")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public IActionResult HealthCheck()
        {
            return Ok(new { 
                Service = "RecommendationService", 
                Status = "Healthy", 
                Timestamp = DateTime.UtcNow,
                Version = "2.0",
                Features = new[] { "GameBased", "SimilarGames", "GenreBased", "Personalized", "DeveloperBased" }
            });
        }
    }
}