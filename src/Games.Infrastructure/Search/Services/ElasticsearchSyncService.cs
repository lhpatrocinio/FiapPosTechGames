using Games.Application.Repository;
using Games.Infrastructure.Search.Interfaces;
using Games.Infrastructure.Search.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Games.Infrastructure.Search.Services
{
    public class ElasticsearchSyncService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ElasticsearchSyncService> _logger;

        public ElasticsearchSyncService(
            IServiceProvider serviceProvider, 
            ILogger<ElasticsearchSyncService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(5000, stoppingToken);

            using var scope = _serviceProvider.CreateScope();
            var elasticsearchService = scope.ServiceProvider.GetRequiredService<IElasticsearchService>();
            var gamesRepository = scope.ServiceProvider.GetRequiredService<IGamesRepository>();

            try
            {
                _logger.LogInformation("Verificando índice Elasticsearch...");
                await elasticsearchService.EnsureIndexExistsAsync();

                _logger.LogInformation("Iniciando sincronização inicial dos jogos...");
                var games = await gamesRepository.GetAllAsync();
                
                if (games.Any())
                {
                    var gameDocuments = games.Select(GameDocument.FromGame).ToList();
                    await elasticsearchService.IndexGamesAsync(gameDocuments);
                    _logger.LogInformation("Sincronização inicial concluída: {Count} jogos indexados", gameDocuments.Count);
                }
                else
                {
                    _logger.LogWarning("Nenhum jogo encontrado para sincronização inicial");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante sincronização inicial com Elasticsearch");
            }
        }
    }
}
