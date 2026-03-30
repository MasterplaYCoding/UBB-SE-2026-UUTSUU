using System.Collections.Generic;
using SearchAndBook.Domain;

namespace SearchAndBook.Repositories
{
    public interface IRentalsRepository : IRepository<TimeRange>
    {
        List<TimeRange> GetUnavailableRanges(int gameId);
        bool CheckAvailability(TimeRange range, int gameId);
    }
}
