using Games.Application.Repository;
using Games.Infrastructure.DataBase.Repository;
using Games.Infrastructure.Search.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Games.Infrastructure
{
    public static class InfraBootstrapper
    {
        public static void Register(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IGamesRepository, GamesRepository>();
            services.AddElasticsearch(configuration);
        }
    }
}
