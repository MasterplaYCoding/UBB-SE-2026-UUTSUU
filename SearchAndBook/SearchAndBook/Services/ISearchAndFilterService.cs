using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SearchAndBook.Shared;

namespace SearchAndBook.Services
{
    /// <summary>
    /// Provides search and filtering capabilities for games.
    /// </summary>
    public interface ISearchAndFilterService
    {
        /// <summary>
        /// Searches for games based on the provided filter criteria.
        /// </summary>
        /// <param name="filter">The criteria to filter the games.</param>
        /// <returns>An array of games matching the filter criteria.</returns>
        GameDTO[] Search(FilterCriteria filter);

        /// <summary>
        /// Retrieves a feed of games available tonight for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>An array of games available tonight.</returns>
        GameDTO[] GetFeedAvailableTonight(int userId);

        /// <summary>
        /// Retrieves a feed of other games for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>An array of other games.</returns>
        GameDTO[] GetFeedOthers(int userId);

        GameDTO[] ApplyFilters(GameDTO[] games, FilterCriteria filter);
    }
}
