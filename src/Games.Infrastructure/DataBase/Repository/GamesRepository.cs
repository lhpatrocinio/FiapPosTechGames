using Games.Application.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Games.Infrastructure.DataBase.Repository
{
    internal class GamesRepository : IGamesRepository
    {
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
