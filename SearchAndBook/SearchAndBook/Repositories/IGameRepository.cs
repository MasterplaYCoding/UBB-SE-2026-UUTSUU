using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SearchAndBook.Shared;
using SearchAndBook.Domain;

namespace SearchAndBook.Repositories
{
    internal interface IGameRepository
    {
        public Game[] getByFilter(FilterCriteria filter);
    }
}
