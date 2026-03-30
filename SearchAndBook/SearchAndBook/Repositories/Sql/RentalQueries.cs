namespace SearchAndBook.Repositories.Sql;

public static class RentalQueries
{
    public const string GetRentalsByGameId = @"
        SELECT
            r.rental_id,
            r.game_id,
            r.renter_id,
            r.owner_id,
            r.start_date,
            r.end_date,
            r.total_price
        FROM dbo.Rentals r
        WHERE r.game_id = @GameId
        ORDER BY r.start_date;";

    public const string GetUnavailablePeriodsByGameId = @"
        SELECT
            r.start_date,
            r.end_date
        FROM dbo.Rentals r
        WHERE r.game_id = @GameId
        ORDER BY r.start_date;";

    public const string HasOverlappingRental = @"
        SELECT TOP 1 1
        FROM dbo.Rentals r
        WHERE r.game_id = @GameId
          AND r.start_date < @RequestedEndDate
          AND r.end_date > @RequestedStartDate;";

    public const string GetRentalsForUser = @"
        SELECT
            r.rental_id,
            r.game_id,
            r.renter_id,
            r.owner_id,
            r.start_date,
            r.end_date,
            r.total_price
        FROM dbo.Rentals r
        WHERE r.renter_id = @UserId
        ORDER BY r.start_date DESC;";

    public const string GetRentalRangeById = @"
    SELECT
        r.start_date,
        r.end_date
    FROM dbo.Rentals r
    WHERE r.rental_id = @RentalId;";

    public const string GetAllRentalRanges = @"
    SELECT
        r.start_date,
        r.end_date
    FROM dbo.Rentals r;";
}