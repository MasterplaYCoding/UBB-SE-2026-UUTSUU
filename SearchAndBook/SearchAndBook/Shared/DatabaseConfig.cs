using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchAndBook.Shared
{
    internal class DatabaseConfig
    {
        public const string ConnectionString =
            @"Server=(localdb)\MSSQLLocalDB;Database=BoardGamesRentMockDb;Trusted_Connection=True;TrustServerCertificate=True;";
    }
}
