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
        public GameDTO[] search(FilterCriteria filter);
        public GameDTO[] getFeedAvailableTonight(int userId);
        public GameDTO[] getFeedOthers(int userId);
    }
}
