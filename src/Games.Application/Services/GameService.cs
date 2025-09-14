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

        public Task AddAsync(Game game)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Game>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Game?> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Game game)
        {
            throw new NotImplementedException();
        }
    }
}
