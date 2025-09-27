using Games.Application.Repository;
using Games.Domain.Entities;
using Games.Infrastructure.DataBase.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.DataBase.Repository
{
    public class GamesRepository : IGamesRepository
    {
        private readonly ApplicationDbContext _context;
        public GamesRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task AddAsync(Games.Domain.Entities.Game entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(Games.Domain.Entities.Game entity)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Games.Domain.Entities.Game>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Games.Domain.Entities.Game> GetByIdAsync(object id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Game>> ListGamesFree()
        {
            return await _context.Set<Game>().Where(x => x.IsFree == true).ToListAsync();
        }

        public IQueryable<Games.Domain.Entities.Game> Query()
        {
            throw new NotImplementedException();
        }

        public Task SaveChangesAsync()
        {
            throw new NotImplementedException();
        }

        public void Update(Games.Domain.Entities.Game entity)
        {
            throw new NotImplementedException();
        }
    }
}
