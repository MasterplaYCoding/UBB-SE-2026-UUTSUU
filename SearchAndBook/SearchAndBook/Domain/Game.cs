using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchAndBook.Domain
{
    public class Game
    {
        public int GameId { get; set; }
        public int OwnerId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int MaximumPlayerNumber { get; set; }
        public int MinimumPlayerNumber { get; set; }
        public string Description { get; set; }
        public byte[]? Image {  get; set; }
        public bool IsActive { get; set; }
       
    }
}
