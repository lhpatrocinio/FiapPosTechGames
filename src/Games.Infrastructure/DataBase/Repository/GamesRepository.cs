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
    }
}
