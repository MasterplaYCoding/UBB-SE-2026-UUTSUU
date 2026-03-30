namespace SearchAndBook.Repositories.Sql;

public static class UserQueries
{
    public const string GetUserById = @"
        SELECT
            u.user_id,
            u.username,
            u.display_name,
            u.email,
            u.password_hash,
            u.phone_number,
            u.avatar_url,
            u.is_suspended,
            u.created_at,
            u.updated_at,
            u.street_name,
            u.street_number,
            u.city,
            u.country
        FROM dbo.Users u
        WHERE u.user_id = @UserId;";

    public const string GetCurrentUserByUsername = @"
        SELECT
            u.user_id,
            u.username,
            u.display_name,
            u.email,
            u.password_hash,
            u.phone_number,
            u.avatar_url,
            u.is_suspended,
            u.created_at,
            u.updated_at,
            u.street_name,
            u.street_number,
            u.city,
            u.country
        FROM dbo.Users u
        WHERE u.username = @Username;";

    public const string GetGameOwnerByGameId = @"
        SELECT
            u.user_id,
            u.username,
            u.display_name,
            u.email,
            u.password_hash,
            u.phone_number,
            u.avatar_url,
            u.is_suspended,
            u.created_at,
            u.updated_at,
            u.street_name,
            u.street_number,
            u.city,
            u.country
        FROM dbo.Users u
        INNER JOIN dbo.Games g ON g.owner_id = u.user_id
        WHERE g.game_id = @GameId;";

    public const string GetAllUsers = @"
        SELECT
            u.user_id,
            u.username,
            u.display_name,
            u.email,
            u.password_hash,
            u.phone_number,
            u.avatar_url,
            u.is_suspended,
            u.created_at,
            u.updated_at,
            u.street_name,
            u.street_number,
            u.city,
            u.country
        FROM dbo.Users u;";
}