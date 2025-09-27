using Asp.Versioning;
using AutoMapper;
using Core.PosTech8Nett.Api.Domain.Model.Game;
using Games.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Games.Api.Controllers.V1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class GamesController : ControllerBase
    {
        private readonly IGameService _gameService;
        private readonly IMapper _mapper;

        public GamesController(IGameService gameService, IMapper mapper)
        {
            _gameService = gameService;
            _mapper = mapper;
        }

        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(IEnumerable<GameResponse>))]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Authorize(Roles = "Games")]
        [HttpGet("list")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            var result = await _gameService.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<GameResponse>>(result));
        }

        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(GameResponse))]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Authorize(Roles = "Games")]
        [HttpGet("getbyid")]
        public async Task<IActionResult> GetById([FromQuery] Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _gameService.GetByIdAsync(id);
            if (result is null)
                return NotFound();

            return Ok(_mapper.Map<GameResponse>(result));
        }

        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] GameRequest request, CancellationToken cancellationToken = default)
        {
            var entity = _mapper.Map<Game>(request);
            await _gameService.AddAsync(entity);

            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, null);
        }

        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Authorize(Roles = "Admin")]
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromQuery] Guid id, [FromBody] GameRequest request, CancellationToken cancellationToken = default)
        {
            var game = _mapper.Map<Game>(request);
            game.Id = id;
            await _gameService.UpdateAsync(game);
            return NoContent();
        }

        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromQuery] Guid id, CancellationToken cancellationToken = default)
        {
            await _gameService.DeleteAsync(id);
            return NoContent();
        }
    }
}