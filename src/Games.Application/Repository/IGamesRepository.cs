using Games.Domain.Entities;

namespace Games.Application.Repository
{
    public interface IGamesRepository
    {
        Task<IEnumerable<Game>> GetAllAsync();
        Task<Game> GetByIdAsync(Guid id);
        Task AddAsync(Game entity);
        Task UpdateAsync(Game entity);
        Task DeleteAsync(Game entity);
        Task SaveChangesAsync();
        IQueryable<Game> Query();
        Task<IEnumerable<Game>> ListGamesFree();
    }
}
