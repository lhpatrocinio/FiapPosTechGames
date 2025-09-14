using Games.Application.Repository;
using Games.Infrastructure.DataBase.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Games.Infrastructure
{
    public static class InfraBootstrapper
    {
        public static void Register(IServiceCollection services)
        {
            services.AddTransient<IGamesRepository, GamesRepository>();
        }
    }
}
