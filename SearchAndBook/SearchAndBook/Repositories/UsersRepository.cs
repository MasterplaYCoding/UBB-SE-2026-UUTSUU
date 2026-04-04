using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using SearchAndBook.Domain;
using SearchAndBook.Repositories.Sql;
using SearchAndBook.Shared;

namespace SearchAndBook.Repositories;

/// How ADO.NET handles connections : 
/// - When you write using var connection = new SqlConnection(...) and call .Open(), Microsoft 
/// checks the pool, so the pool of connections is handled by .net
/// - If there is a free connection, it gives it to you.
/// - When your "using" block finishes, it calls .Close().
/// - Microsoft intercepts your .Close() command. It doesn't actually destroy the connection 
/// to the database. It just wipes the data clean and parks it back in the hidden pool for 
/// the next person to use.


public class UsersRepository : IUsersRepository
{
    // Get User by id
    public User? Get(int id)
    {
        try
        {
            using var connection = new SqlConnection(DatabaseConfig.ConnectionString);
            connection.Open();

            using var command = new SqlCommand(UserQueries.GetUserById, connection);
            command.Parameters.AddWithValue("@UserId", id);

            using var reader = command.ExecuteReader();

            if (!reader.Read())
            {
                return null;
            }

            return MapUser(reader);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public List<User> GetAll()
    {
        try
        {
            var users = new List<User>();

            using var connection = new SqlConnection(DatabaseConfig.ConnectionString);
            connection.Open();

            using var command = new SqlCommand(UserQueries.GetAllUsers, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                users.Add(MapUser(reader));
            }

            return users;
        }
        catch (Exception)
        {
            throw;
        }
    }

    private static User MapUser(SqlDataReader reader)
    {
        return new User
        {
            UserId = Convert.ToInt32(reader["user_id"]),
            Username = Convert.ToString(reader["username"]) ?? string.Empty,
            DisplayName = Convert.ToString(reader["display_name"]) ?? string.Empty,
            Email = Convert.ToString(reader["email"]) ?? string.Empty,
            PasswordHash = Convert.ToString(reader["password_hash"]) ?? string.Empty,
            PhoneNumber = reader["phone_number"] == DBNull.Value ? null : Convert.ToString(reader["phone_number"]),
            AvatarUrl = reader["avatar_url"] == DBNull.Value ? null : Convert.ToString(reader["avatar_url"]),
            IsSuspended = Convert.ToBoolean(reader["is_suspended"]),
            CreatedAt = Convert.ToDateTime(reader["created_at"]),
            UpdatedAt = reader["updated_at"] == DBNull.Value ? null : Convert.ToDateTime(reader["updated_at"]),
            StreetName = reader["street_name"] == DBNull.Value ? null : Convert.ToString(reader["street_name"]),
            StreetNumber = reader["street_number"] == DBNull.Value ? null : Convert.ToString(reader["street_number"]),
            City = Convert.ToString(reader["city"]) ?? string.Empty,
            Country = Convert.ToString(reader["country"]) ?? string.Empty
        };
    }
}