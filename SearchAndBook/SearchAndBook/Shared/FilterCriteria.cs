using SearchAndBook.Domain;

namespace SearchAndBook.Shared;

public class FilterCriteria
{
    public string? Name { get; set; }
    public string? City { get; set; }
    public TimeRange? AvailabilityRange { get; set; }
    public decimal? MaximumPrice { get; set; }
    public int? PlayerCount { get; set; }
    public bool? IsSortedAscending { get; set; }
    public int? UserId { get; set; }
}