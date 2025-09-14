using System.Collections.Generic;

namespace Games.Domain.Entities
{
    public class GenreTypes
    {
        public int Id { get; set; }
        public string Description { get; set; }

        public ICollection<GameGenre> GameGenres { get; set; }
    }
}