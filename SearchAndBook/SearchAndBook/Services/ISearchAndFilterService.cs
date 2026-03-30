using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SearchAndBook.Shared;

namespace SearchAndBook.Services
{
    internal interface ISearchAndFilterService
    {
        public GameDTO[] Search(FilterCriteria filter);
        public GameDTO[] GetFeedAvailableTonight(int userId);
        public GameDTO[] GetFeedOthers(int userId);

        public GameDTO[] ApplyFilters(GameDTO[] sourceGames, FilterCriteria filter);
    }
}
