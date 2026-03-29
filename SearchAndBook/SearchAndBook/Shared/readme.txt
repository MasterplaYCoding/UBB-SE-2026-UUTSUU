GameDTO
BookingDTO
FilterCriteria
TimeRange

How to use DatabaseConfig in repo:

using Microsoft.Data.SqlClient;

using var connection = new SqlConnection(DatabaseConfig.ConnectionString);
connection.Open();