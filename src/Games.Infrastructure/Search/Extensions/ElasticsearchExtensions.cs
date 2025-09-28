using Games.Infrastructure.Search.HealthChecks;
using Games.Infrastructure.Search.Interfaces;
using Games.Infrastructure.Search.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace Games.Infrastructure.Search.Extensions
{
    public static class ElasticsearchExtensions
    {
        public static IServiceCollection AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
        {
            var elasticUri = configuration.GetConnectionString("Elasticsearch") ?? "http://localhost:9200";
            
            var settings = new ConnectionSettings(new Uri(elasticUri))
                .DefaultIndex("games")
                .DisableDirectStreaming()
                .RequestTimeout(TimeSpan.FromSeconds(30))
                .MaximumRetries(3)
                .DefaultMappingFor<Models.GameDocument>(m => m
                    .IndexName("games")
                    .IdProperty(p => p.Id)
                );

            var client = new ElasticClient(settings);
            
            services.AddSingleton<IElasticClient>(client);
            services.AddScoped<IElasticsearchService, ElasticsearchService>();
            services.AddHostedService<ElasticsearchSyncService>();
            
            services.AddHealthChecks()
                .AddCheck<ElasticsearchHealthCheck>("elasticsearch", tags: new[] { "search", "elasticsearch" });

            return services;
        }
    }
}
