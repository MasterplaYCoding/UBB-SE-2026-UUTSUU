using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SearchAndBook.Domain;

namespace SearchAndBook.Shared
{
    internal class FilterCriteria
    {
        public string? Name {  get; set; }
        public string? City { get; set; }
        public TimeRange AvailabilityRange { get; set; }
        public int MaximumPrice { get; set; }
        public int MinimumPrice { get; set; }
        public bool IsSortedAscending { get; set; }
        public int UserId { get; set; }
    }
}
