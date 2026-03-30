using SearchAndBook.Shared;
using SearchAndBook.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchAndBook.Repositories
{
    internal class GameRepository : IGameRepository
    {
        public Game[] getByFilter(FilterCriteria filter)
        {
            // Implement logic to retrieve games based on the filter criteria from the database
            // This is a placeholder implementation and should be replaced with actual logic
            return Array.Empty<Game>();

        }
    }
}
