namespace Games.Api.Dtos.Responses
{
    public class SearchResultResponse
    {
        public List<GameDocumentResponse> Games { get; set; } = new();
        public long Total { get; set; }
        public int From { get; set; }
        public int Size { get; set; }
    }

    public class GameDocumentResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Rating { get; set; }
        public string Developer { get; set; } = string.Empty;
        public string IndicatedAgeRating { get; set; } = string.Empty;
        public decimal HourPlayed { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public List<string> Genres { get; set; } = new();
    }
}