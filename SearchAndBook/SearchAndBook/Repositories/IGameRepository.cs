using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SearchAndBook.Shared;

namespace SearchAndBook.Repositories
{
    internal interface IGameRepository
    {
        public List<GameDTO> getByFilter(FilterCriteria filter);
    }
}
