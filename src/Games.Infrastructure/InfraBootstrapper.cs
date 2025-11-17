using Games.Application.Repository;
using Games.Infrastructure.DataBase.Repository;
using Games.Infrastructure.Search.Extensions;
using Games.Infrastructure.Search.Interfaces;
using Games.Infrastructure.Search.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Games.Infrastructure
{
    public static class InfraBootstrapper
    {
        public static void Register(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IGamesRepository, GamesRepository>();
            services.AddTransient<IElasticsearchAnalyticsService, ElasticsearchAnalyticsService>();
            services.AddTransient<IElasticsearchRecommendationService, ElasticsearchRecommendationService>();
            services.AddElasticsearch(configuration);
        }
    }
}
