using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Games.Domain.Entities
{
    public class GameLibrary
    {
        public Guid IdLibrary { get; set; }
        public Guid IdGame { get; set; }

        public Library Library { get; set; }
        public Game Game { get; set; }
    }
}
