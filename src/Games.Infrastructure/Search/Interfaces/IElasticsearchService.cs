using Games.Infrastructure.Search.Models;

namespace Games.Infrastructure.Search.Interfaces
{
    public interface IElasticsearchService
    {
        Task<bool> IndexGameAsync(GameDocument game);
        Task<bool> IndexGamesAsync(IEnumerable<GameDocument> games);
        Task<bool> UpdateGameAsync(GameDocument game);
        Task<bool> DeleteGameAsync(Guid gameId);
        Task<SearchResult<GameDocument>> SearchGamesAsync(GameSearchRequest request);
        Task<bool> EnsureIndexExistsAsync();
    }

    public class GameSearchRequest
    {
        public string? Query { get; set; }
        public List<string>? Genres { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? MinRating { get; set; }
        public string? Developer { get; set; }
        public int From { get; set; } = 0;
        public int Size { get; set; } = 10;
    }

    public class SearchResult<T>
    {
        public List<T> Documents { get; set; } = new();
        public long Total { get; set; }
        public int From { get; set; }
        public int Size { get; set; }
    }
}
