using Games.Domain.Entities;

namespace Games.Application.Repository
{
    public interface IGamesRepository
    {
        Task<IEnumerable<Game>> GetAllAsync();
        Task<Game> GetByIdAsync(object id);
        Task AddAsync(Game entity);
        void Update(Game entity);
        void Delete(Game entity);
        Task SaveChangesAsync();
        IQueryable<Game> Query();
        Task<IEnumerable<Game>> ListGamesFree();
    }
}
