namespace SearchAndBook.Repositories.Sql;

public static class GameQueries
{
    public const string GetGameById = @"
        SELECT
            g.game_id,
            g.name,
            g.price,
            g.minimum_player_number,
            g.maximum_player_number,
            g.description,
            g.image,
            g.is_active,
            g.owner_id
        FROM dbo.Games g
        WHERE g.game_id = @GameId;";

    public const string GetGameWithOwnerById = @"
        SELECT
            g.game_id,
            g.name,
            g.price,
            g.minimum_player_number,
            g.maximum_player_number,
            g.description,
            g.image,
            g.is_active,
            g.owner_id,

            u.user_id,
            u.username,
            u.display_name,
            u.email,
            u.phone_number,
            u.avatar_url,
            u.is_suspended,
            u.created_at,
            u.updated_at,
            u.street_name,
            u.street_number,
            u.city,
            u.country
        FROM dbo.Games g
        INNER JOIN dbo.Users u ON g.owner_id = u.user_id
        WHERE g.game_id = @GameId;";

    public const string GetAllActiveGamesWithOwner = @"
        SELECT
            g.game_id,
            g.name,
            g.price,
            g.minimum_player_number,
            g.maximum_player_number,
            g.description,
            g.image,
            g.is_active,
            g.owner_id,

            u.user_id,
            u.username,
            u.display_name,
            u.email,
            u.phone_number,
            u.avatar_url,
            u.is_suspended,
            u.created_at,
            u.updated_at,
            u.street_name,
            u.street_number,
            u.city,
            u.country
        FROM dbo.Games g
        INNER JOIN dbo.Users u ON g.owner_id = u.user_id
        WHERE g.is_active = 1
            AND g.owner_id <> @UserId;";

    public const string GetAvailableGamesForDateRange = @"
        SELECT
            g.game_id,
            g.name,
            g.price,
            g.minimum_player_number,
            g.maximum_player_number,
            g.description,
            g.image,
            g.is_active,
            g.owner_id,

            u.user_id,
            u.username,
            u.display_name,
            u.email,
            u.phone_number,
            u.avatar_url,
            u.is_suspended,
            u.created_at,
            u.updated_at,
            u.street_name,
            u.street_number,
            u.city,
            u.country
        FROM dbo.Games g
        INNER JOIN dbo.Users u ON g.owner_id = u.user_id
        WHERE g.is_active = 1
            AND g.owner_id <> @UserId
          AND NOT EXISTS
          (
              SELECT 1
              FROM dbo.Rentals r
              WHERE r.game_id = g.game_id
                AND r.start_date < @RequestedEndDate
                AND r.end_date > @RequestedStartDate
          );";

    public const string SearchAvailableGames = @"
        SELECT
            g.game_id,
            g.name,
            g.price,
            g.minimum_player_number,
            g.maximum_player_number,
            g.description,
            g.image,
            g.is_active,
            g.owner_id,

            u.user_id,
            u.username,
            u.display_name,
            u.email,
            u.phone_number,
            u.avatar_url,
            u.is_suspended,
            u.created_at,
            u.updated_at,
            u.street_name,
            u.street_number,
            u.city,
            u.country
        FROM dbo.Games g
        INNER JOIN dbo.Users u ON g.owner_id = u.user_id
        WHERE g.is_active = 1
          AND g.owner_id <> @UserId
          AND (@Title IS NULL OR g.name LIKE '%' + @Title + '%')
          AND (@City IS NULL OR u.city = @City)
          AND NOT EXISTS
          (
              SELECT 1
              FROM dbo.Rentals r
              WHERE r.game_id = g.game_id
                AND r.start_date < @RequestedEndDate
                AND r.end_date > @RequestedStartDate
          );";

    public const string SearchAvailableGamesWithFilters = @"
        SELECT
            g.game_id,
            g.name,
            g.price,
            g.minimum_player_number,
            g.maximum_player_number,
            g.description,
            g.image,
            g.is_active,
            g.owner_id,

            u.user_id,
            u.username,
            u.display_name,
            u.email,
            u.phone_number,
            u.avatar_url,
            u.is_suspended,
            u.created_at,
            u.updated_at,
            u.street_name,
            u.street_number,
            u.city,
            u.country
        FROM dbo.Games g
        INNER JOIN dbo.Users u ON g.owner_id = u.user_id
        WHERE g.is_active = 1
          AND g.owner_id <> @UserId
          AND (@Title IS NULL OR g.name LIKE '%' + @Title + '%')
          AND (@City IS NULL OR u.city = @City)
          AND (@MaxPrice IS NULL OR g.price <= @MaxPrice)
          AND (@PlayerCount IS NULL OR @PlayerCount BETWEEN g.minimum_player_number AND g.maximum_player_number)
          AND (
            @RequestedStartDate IS NULL
            OR @RequestedEndDate IS NULL
            OR NOT EXISTS (
                SELECT 1
                FROM dbo.Rentals r
                WHERE r.game_id = g.game_id
                AND r.start_date < @RequestedEndDate
                AND r.end_date > @RequestedStartDate
            )
        );";

    public const string GetGamesOwnedByUser = @"
        SELECT
            g.game_id,
            g.name,
            g.price,
            g.minimum_player_number,
            g.maximum_player_number,
            g.description,
            g.image,
            g.is_active,
            g.owner_id
        FROM dbo.Games g
        WHERE g.owner_id = @OwnerId;";
}