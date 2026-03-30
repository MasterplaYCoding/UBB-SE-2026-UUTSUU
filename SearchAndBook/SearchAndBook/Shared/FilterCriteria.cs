using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SearchAndBook.Domain;

namespace SearchAndBook.Shared
{
    internal class FilterCriteria
    {
        public string? Name {  get; set; }
        public string? City { get; set; }
        public TimeRange? AvailabilityRange { get; set; }

        public int? MinimumPlayers { get; set; }
        public decimal? MaximumPrice { get; set; }
        public decimal? MinimumPrice { get; set; }
        public SortOption SortOption { get; set; }
        public int? UserId { get; set; }

        public FilterCriteria() {
            Reset();
        }

        public void Reset() {
            Name = null;
            City = null;
            AvailabilityRange = null;
            MinimumPrice = null;
            MaximumPrice = null;
            MinimumPlayers = null;
            SortOption = SortOption.None;
            UserId = null;
        }

        public bool IsEmpty() { 
            return string.IsNullOrWhiteSpace(Name) && string.IsNullOrWhiteSpace(City) && AvailabilityRange == null && MinimumPlayers==null && MinimumPrice==null && MaximumPrice==null && SortOption == SortOption.None;
        }

        public bool HasValidAvailabilityRange() {
            return AvailabilityRange == null || AvailabilityRange.IsValid();
        }
    }
}
