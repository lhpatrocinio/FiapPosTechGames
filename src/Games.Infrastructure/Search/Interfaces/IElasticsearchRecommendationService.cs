using Games.Infrastructure.Search.Models;

namespace Games.Infrastructure.Search.Interfaces
{
    public interface IElasticsearchRecommendationService
    {
        /// <summary>
        /// Recomenda jogos similares com base em um jogo específico
        /// </summary>
        /// <param name="gameId">ID do jogo base para recomendações</param>
        /// <param name="maxResults">Número máximo de recomendações (padrão: 10)</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Lista de jogos recomendados com scores de similaridade</returns>
        Task<GameRecommendationResult> GetGameBasedRecommendationsAsync(Guid gameId, int maxResults = 10, CancellationToken cancellationToken = default);

        /// <summary>
        /// Encontra jogos similares baseado em critérios de busca
        /// </summary>
        /// <param name="searchQuery">Texto de busca para encontrar jogos similares</param>
        /// <param name="genres">Gêneros preferidos (opcional)</param>
        /// <param name="minRating">Rating mínimo (opcional)</param>
        /// <param name="maxResults">Número máximo de resultados (padrão: 15)</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Lista de jogos similares com scores</returns>
        Task<SimilarGamesResult> FindSimilarGamesAsync(string searchQuery, List<string>? genres = null, decimal? minRating = null, int maxResults = 15, CancellationToken cancellationToken = default);

        /// <summary>
        /// Recomenda jogos com base em gêneros preferidos
        /// </summary>
        /// <param name="preferredGenres">Lista de gêneros preferidos</param>
        /// <param name="minRating">Rating mínimo desejado (opcional)</param>
        /// <param name="maxPrice">Preço máximo desejado (opcional)</param>
        /// <param name="maxResults">Número máximo de recomendações por gênero (padrão: 5)</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Recomendações organizadas por gênero</returns>
        Task<GenreBasedRecommendationResult> GetGenreBasedRecommendationsAsync(List<string> preferredGenres, decimal? minRating = null, decimal? maxPrice = null, int maxResults = 5, CancellationToken cancellationToken = default);

        /// <summary>
        /// Recomendações personalizadas baseadas no perfil do usuário
        /// </summary>
        /// <param name="userProfile">Perfil de preferências do usuário</param>
        /// <param name="maxResults">Número máximo de recomendações (padrão: 20)</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Lista personalizada de jogos recomendados</returns>
        Task<UserPreferencesRecommendationResult> GetPersonalizedRecommendationsAsync(UserPreferencesProfile userProfile, int maxResults = 20, CancellationToken cancellationToken = default);

        /// <summary>
        /// Recomenda jogos com base em desenvolvedor similar
        /// </summary>
        /// <param name="developer">Nome do desenvolvedor</param>
        /// <param name="excludeGameIds">IDs de jogos para excluir das recomendações</param>
        /// <param name="maxResults">Número máximo de recomendações (padrão: 10)</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Jogos do mesmo desenvolvedor ou similares</returns>
        Task<SimilarGamesResult> GetDeveloperBasedRecommendationsAsync(string developer, List<Guid>? excludeGameIds = null, int maxResults = 10, CancellationToken cancellationToken = default);
    }
}