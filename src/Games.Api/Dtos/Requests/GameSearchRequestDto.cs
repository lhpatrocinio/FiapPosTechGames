using System.ComponentModel.DataAnnotations;

namespace Games.Api.Dtos.Requests
{
    public class GameSearchRequestDto
    {
        public string? Query { get; set; }
        public List<string>? Genres { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Preço mínimo deve ser maior ou igual a 0")]
        public decimal? MinPrice { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Preço máximo deve ser maior ou igual a 0")]
        public decimal? MaxPrice { get; set; }
        
        [Range(0, 5, ErrorMessage = "Rating deve estar entre 0 e 5")]
        public decimal? MinRating { get; set; }
        
        public string? Developer { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "From deve ser maior ou igual a 0")]
        public int From { get; set; } = 0;
        
        [Range(1, 100, ErrorMessage = "Size deve estar entre 1 e 100")]
        public int Size { get; set; } = 10;
    }
}