using System;

namespace Games.Domain.Entities
{
    public class GameGenre
    {
        public int IdGenre { get; set; }
        public Guid IdGame { get; set; }

        public GenreTypes GenreType { get; set; }
        public Game Game { get; set; }
    }
}