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
        public GameDTO[] getByFilter(FilterCriteria filter);
    }
}
