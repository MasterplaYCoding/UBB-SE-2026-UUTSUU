using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using SearchAndBook.Domain;
using SearchAndBook.Repositories.Sql;
using SearchAndBook.Shared;

namespace SearchAndBook.Repositories;

public class GamesRepository : IGamesRepository
{
    public Game? Get(int id)
    {
        using var connection = new SqlConnection(DatabaseConfig.ConnectionString);
        connection.Open();

        using var command = new SqlCommand(GameQueries.GetGameById, connection);
        command.Parameters.AddWithValue("@GameId", id);

        using var reader = command.ExecuteReader();

        if (!reader.Read())
        {
            return null;
        }

        return MapGame(reader);
    }

    public List<Game> GetAll()
    {
        return GetAllActiveGames(-1);
    }

    public List<Game> GetByFilter(FilterCriteria filter)
    {
        var games = new List<Game>();

        using var connection = new SqlConnection(DatabaseConfig.ConnectionString);
        connection.Open();

        using var command = new SqlCommand(GameQueries.SearchAvailableGamesWithFilters, connection);
        command.Parameters.AddWithValue("@Title", string.IsNullOrWhiteSpace(filter.Name) ? DBNull.Value : filter.Name);
        command.Parameters.AddWithValue("@City", string.IsNullOrWhiteSpace(filter.City) ? DBNull.Value : filter.City);
        command.Parameters.AddWithValue("@MaxPrice", filter.MaximumPrice.HasValue ? filter.MaximumPrice.Value : DBNull.Value);
        command.Parameters.AddWithValue("@PlayerCount", filter.PlayerCount.HasValue ? filter.PlayerCount.Value : DBNull.Value);
        command.Parameters.AddWithValue("@RequestedStartDate",
            filter.AvailabilityRange != null ? filter.AvailabilityRange.StartTime : DBNull.Value);
        command.Parameters.AddWithValue("@RequestedEndDate",
            filter.AvailabilityRange != null ? filter.AvailabilityRange.EndTime : DBNull.Value);
        command.Parameters.AddWithValue("@UserId", filter.UserId.HasValue ? filter.UserId.Value : -1);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            games.Add(MapGame(reader));
        }

        return games;
    }

    public List<Game> GetForFeedAvailableTonight(int userId)
    {
        var games = new List<Game>();

        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        using var connection = new SqlConnection(DatabaseConfig.ConnectionString);
        connection.Open();

        using var command = new SqlCommand(GameQueries.GetAvailableGamesForDateRange, connection);
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@RequestedStartDate", today);
        command.Parameters.AddWithValue("@RequestedEndDate", tomorrow);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            games.Add(MapGame(reader));
        }

        return games;
    }

    public List<Game> GetForFeedOthers(int userId)
    {
        var allActiveGames = GetAllActiveGames(userId);
        var availableTonightGames = GetForFeedAvailableTonight(userId);

        var availableTonightIds = availableTonightGames
            .Select(game => game.GameId)
            .ToHashSet();

        return allActiveGames
            .Where(game => !availableTonightIds.Contains(game.GameId))
            .ToList();
    }

    private List<Game> GetAllActiveGames(int userId)
    {
        var games = new List<Game>();

        using var connection = new SqlConnection(DatabaseConfig.ConnectionString);
        connection.Open();

        using var command = new SqlCommand(GameQueries.GetAllActiveGamesWithOwner, connection);
        command.Parameters.AddWithValue("@UserId", userId);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            games.Add(MapGame(reader));
        }

        return games;
    }

    private static Game MapGame(SqlDataReader reader)
    {
        return new Game
        {
            GameId = Convert.ToInt32(reader["game_id"]),
            Name = Convert.ToString(reader["name"]) ?? string.Empty,
            Price = Convert.ToDecimal(reader["price"]),
            MinimumPlayerNumber = Convert.ToInt32(reader["minimum_player_number"]),
            MaximumPlayerNumber = Convert.ToInt32(reader["maximum_player_number"]),
            Description = Convert.ToString(reader["description"]) ?? string.Empty,
            Image = reader["image"] == DBNull.Value ? null : (byte[])reader["image"],
            IsActive = Convert.ToBoolean(reader["is_active"]),
            OwnerId = Convert.ToInt32(reader["owner_id"])
        };
    }
}