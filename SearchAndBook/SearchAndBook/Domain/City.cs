using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchAndBook.Domain
{
    public class City
    {
        public string MainName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<string> Names { get; set; }
    }
}
