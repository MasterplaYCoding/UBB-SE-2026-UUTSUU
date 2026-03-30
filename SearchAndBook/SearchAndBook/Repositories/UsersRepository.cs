using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using SearchAndBook.Domain;
using SearchAndBook.Repositories.Sql;
using SearchAndBook.Shared;

namespace SearchAndBook.Repositories;

public class UsersRepository : IUsersRepository
{
    public User? Get(int id)
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

    public List<User> GetAll()
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