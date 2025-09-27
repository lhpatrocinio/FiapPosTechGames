using Games.Domain.Entities;
using Games.Application.Repository;

namespace Games.Application.Services
{
    public class GameService : IGameService
    {
        private readonly IGamesRepository _repository;

        public GameService(IGamesRepository repository)
        {
            _repository = repository;
        }

        public async Task AddAsync(Game game)
        {
            await _repository.AddAsync(game);
        }

        public async Task DeleteAsync(Guid id)
        {
            var game = await _repository.GetByIdAsync(id);
            await _repository.DeleteAsync(game);
        }

        public async Task<IEnumerable<Game>> GetAllAsync()
        {
           return await _repository.GetAllAsync();
        }

        public async Task<Game?> GetByIdAsync(Guid id)
        {
           return await _repository.GetByIdAsync(id);
        }

        public async Task UpdateAsync(Game game)
        {
            await _repository.UpdateAsync(game);
        }
    }
}
