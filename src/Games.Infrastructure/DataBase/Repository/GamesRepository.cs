using Games.Application.Repository;
using Games.Domain.Entities;
using Games.Infrastructure.DataBase.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Games.Infrastructure.DataBase.Repository
{
    public class GamesRepository : IGamesRepository
    {
        private readonly ApplicationDbContext _context;
        public GamesRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Games.Domain.Entities.Game entity)
        {
            await _context.AddAsync(entity);
            await SaveChangesAsync();
        }

        public async Task DeleteAsync(Games.Domain.Entities.Game entity)
        {
            _context.Set<Game>().Remove(entity);
            await SaveChangesAsync();
        }

        public async Task<IEnumerable<Games.Domain.Entities.Game>> GetAllAsync()
        {
            return await _context.Set<Game>()
                .Include(g => g.Genres)
                    .ThenInclude(gg => gg.GenreType)
                .ToListAsync();
        }

        public async Task<Games.Domain.Entities.Game> GetByIdAsync(Guid id)
        {
            return await _context.Set<Game>().Where(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Game>> ListGamesFree()
        {
            return await _context.Set<Game>().Where(x => x.IsFree == true).ToListAsync();
        }

        public IQueryable<Games.Domain.Entities.Game> Query()
        {
            throw new NotImplementedException();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Games.Domain.Entities.Game entity)
        {
            _context.Set<Game>().Update(entity);
            await SaveChangesAsync();
        }

        public async Task<Games.Domain.Entities.Library> GetLibraryByIdAsync(Guid idUser)
        {
            return await _context.Set<Library>().Where(x => x.Id == idUser).FirstOrDefaultAsync();
        }

        public async Task AddLibraryAsync(Games.Domain.Entities.Library entity)
        {
            await _context.AddAsync(entity);
            await SaveChangesAsync();
        }

        public async Task AddGameLibraryAsync(Games.Domain.Entities.GameLibrary entity)
        {        await _context.AddAsync(entity);
            await SaveChangesAsync();
    
        }

        public async Task<IEnumerable<Game>> AddGameLibraryAsync(Guid idUser)
        {
            var userGames = await (
                         from g in _context.Set<Game>()
                         join gl in _context.Set<GameLibrary>() on g.Id equals gl.IdGame
                         join l in _context.Set<Library>() on gl.IdLibrary equals l.Id
                         where l.IdUser == idUser
                         select g
                     ).ToListAsync();

            return userGames;
        }

        public async Task BeginTransactionAsync()
        {
            await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            await _context.Database.CommitTransactionAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _context.Database.RollbackTransactionAsync();
        }
    }
}
