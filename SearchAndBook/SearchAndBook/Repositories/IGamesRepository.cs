using System.Collections.Generic;
using SearchAndBook.Domain;
using SearchAndBook.Shared;

namespace SearchAndBook.Repositories
{
    public interface IGamesRepository : IRepository<Game>
    {
        List<Game> GetByFilter(FilterCriteria filter);
        List<Game> GetForFeedAvailableTonight(int userId);
        List<Game> GetForFeedOthers(int userId);

    }
}
