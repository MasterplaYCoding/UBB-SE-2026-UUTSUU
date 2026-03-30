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
        GameDTO[] Search(FilterCriteria filter);
        GameDTO[] GetFeedAvailableTonight(int userId);
        GameDTO[] GetFeedOthers(int userId);
    }
}
