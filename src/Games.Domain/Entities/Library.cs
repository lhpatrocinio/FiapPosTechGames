using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Games.Domain.Entities
{
    public class Library
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid IdUser { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }
        public ICollection<GameLibrary> GameLibraries { get; set; } = new List<GameLibrary>();
    }
}
