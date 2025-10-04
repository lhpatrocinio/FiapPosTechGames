using Asp.Versioning;
using AutoMapper;
using Games.Api.Dtos.Requests;
using Games.Api.Dtos.Responses;
using Games.Application.Repository;
using Games.Infrastructure.Search.Interfaces;
using Games.Infrastructure.Search.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Games.Api.Controllers.V1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [AllowAnonymous]
    public class SearchController : ControllerBase
    {
        private readonly IElasticsearchService _elasticsearchService;
        private readonly IMapper _mapper;
        private readonly ILogger<SearchController> _logger;
        private readonly IGamesRepository _gamesRepository;

        public SearchController(
            IElasticsearchService elasticsearchService, 
            IMapper mapper,
            ILogger<SearchController> logger,
            IGamesRepository gamesRepository)
        {
            _elasticsearchService = elasticsearchService;
            _mapper = mapper;
            _logger = logger;
            _gamesRepository = gamesRepository;
        }

        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(SearchResultResponse))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [HttpGet("games")]
        public async Task<IActionResult> SearchGames([FromQuery] GameSearchRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var searchRequest = _mapper.Map<GameSearchRequest>(request);
                var result = await _elasticsearchService.SearchGamesAsync(searchRequest);
                
                var response = _mapper.Map<SearchResultResponse>(result);
                
                _logger.LogInformation("Search executed: Query='{Query}', Results={Total}", 
                    request.Query, result.Total);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing search");
                return BadRequest("Erro ao executar busca");
            }
        }

        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        [HttpPost("reindex")]
        public async Task<IActionResult> ReindexGames()
        {
            try
            {
                var indexSuccess = await _elasticsearchService.EnsureIndexExistsAsync();
                
                if (!indexSuccess)
                {
                    return StatusCode(503, "Falha ao configurar índice Elasticsearch");
                }

                var games = await _gamesRepository.GetAllAsync();
                
                if (games.Any())
                {
                    var gameDocuments = games.Select(GameDocument.FromGame).ToList();
                    var indexingSuccess = await _elasticsearchService.IndexGamesAsync(gameDocuments);
                    
                    if (indexingSuccess)
                    {
                        _logger.LogInformation("Reindex completed successfully: {Count} games indexed", gameDocuments.Count);
                        return Ok(new { 
                            message = "Reindex concluído com sucesso", 
                            gamesIndexed = gameDocuments.Count 
                        });
                    }
                    else
                    {
                        return StatusCode(503, "Falha ao indexar jogos no Elasticsearch");
                    }
                }
                else
                {
                    _logger.LogInformation("No games found to index");
                    return Ok(new { 
                        message = "Nenhum jogo encontrado para indexar",
                        gamesIndexed = 0
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during reindex operation");
                return StatusCode(503, "Erro interno durante operação de reindex");
            }
        }
    }
}
