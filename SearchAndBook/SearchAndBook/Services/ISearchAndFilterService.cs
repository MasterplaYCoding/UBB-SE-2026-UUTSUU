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
        public List<GameDTO> search(FilterCriteria filter);
        public List<GameDTO> getFeedAvailableTonight(int userId);
        public List<GameDTO> getFeedOthers(int userId);
    }
}
