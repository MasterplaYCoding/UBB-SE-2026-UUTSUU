using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using SearchAndBook.Domain;
using SearchAndBook.Repositories.Sql;
using SearchAndBook.Shared;

namespace SearchAndBook.Repositories;

public class RentalsRepository : IRentalsRepository
{
    public TimeRange? Get(int id)
    {
        using var connection = new SqlConnection(DatabaseConfig.ConnectionString);
        connection.Open();

        using var command = new SqlCommand(RentalQueries.GetRentalRangeById, connection);
        command.Parameters.AddWithValue("@RentalId", id);

        using var reader = command.ExecuteReader();

        if (!reader.Read())
        {
            return null;
        }

        return new TimeRange(
            Convert.ToDateTime(reader["start_date"]),
            Convert.ToDateTime(reader["end_date"]));
    }

    public List<TimeRange> GetAll()
    {
        var ranges = new List<TimeRange>();

        using var connection = new SqlConnection(DatabaseConfig.ConnectionString);
        connection.Open();

        using var command = new SqlCommand(RentalQueries.GetAllRentalRanges, connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            ranges.Add(new TimeRange(
                Convert.ToDateTime(reader["start_date"]),
                Convert.ToDateTime(reader["end_date"])));
        }

        return ranges;
    }

    public List<TimeRange> GetUnavailableRanges(int gameId)
    {
        var ranges = new List<TimeRange>();

        using var connection = new SqlConnection(DatabaseConfig.ConnectionString);
        connection.Open();

        using var command = new SqlCommand(RentalQueries.GetUnavailablePeriodsByGameId, connection);
        command.Parameters.AddWithValue("@GameId", gameId);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var start = Convert.ToDateTime(reader["start_date"]);
            var end = Convert.ToDateTime(reader["end_date"]);

            ranges.Add(new TimeRange(start, end));
        }

        return ranges;
    }

    public bool CheckAvailability(TimeRange range, int gameId)
    {
        using var connection = new SqlConnection(DatabaseConfig.ConnectionString);
        connection.Open();

        using var command = new SqlCommand(RentalQueries.HasOverlappingRental, connection);
        command.Parameters.AddWithValue("@GameId", gameId);
        command.Parameters.AddWithValue("@RequestedStartDate", range.StartTime);
        command.Parameters.AddWithValue("@RequestedEndDate", range.EndTime);

        var result = command.ExecuteScalar();

        return result == null;
    }
}