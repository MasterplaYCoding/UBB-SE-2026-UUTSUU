using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SearchAndBook.Domain;

namespace SearchAndBook.Repositories
{
    internal interface IRentalRepository
    {
        Rental[] GetByGameId(int gameId);
    }
}
