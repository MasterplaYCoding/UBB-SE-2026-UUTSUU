using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchAndBook.Shared
{
    internal class GameDTO
    {
        public int GameId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Image { get; set; }
        public decimal Price { get; set; }
        public string City { get; set; } = string.Empty;
        public int MaximumPlayerNumber { get; set; }
        public int MinimumPlayerNumber { get; set; }
    }
}
