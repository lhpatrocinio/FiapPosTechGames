using Games.Domain.Entities;

public interface IGameService
{
    Task<IEnumerable<Game>> GetAllAsync();
    Task<Game?> GetByIdAsync(Guid id);
    Task AddAsync(Game game);
    Task UpdateAsync(Game game);
    Task DeleteAsync(Guid id);
}
