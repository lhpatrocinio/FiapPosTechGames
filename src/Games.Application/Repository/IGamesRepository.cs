using Games.Domain.Entities;
using System.Threading.Tasks;

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
        Task AddLibraryAsync(Games.Domain.Entities.Library entity);
        Task<Games.Domain.Entities.Library> GetLibraryByIdAsync(Guid idUser);
        Task AddGameLibraryAsync(Games.Domain.Entities.GameLibrary entity);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
