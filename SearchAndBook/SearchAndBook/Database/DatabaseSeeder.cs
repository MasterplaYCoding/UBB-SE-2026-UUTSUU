using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.IO;

namespace SearchAndBook.Shared;

public static class DatabaseSeeder
{
    public static void SeedGameImages()
    {
        string basePath = AppContext.BaseDirectory;
        string projectRoot = FindProjectRoot(basePath);

        string imageFolder = Path.Combine(projectRoot, "Assets", "SeedImages");

        var imageMap = new Dictionary<int, string>
        {
            { 1, "catan.png" },
            { 2, "monopoly.jpg" },
            { 3, "carcassonne.jpg" },
            { 4, "terraforming_mars.png" }
        };

        using var connection = new SqlConnection(DatabaseConfig.ConnectionString);
        connection.Open();

        foreach (var pair in imageMap)
        {
            int gameId = pair.Key;
            string filePath = Path.Combine(imageFolder, pair.Value);

            if (!File.Exists(filePath))
            {
                continue;
            }

            byte[] imageBytes = File.ReadAllBytes(filePath);

            using var command = new SqlCommand(@"
                UPDATE dbo.Games
                SET image = @Image
                WHERE game_id = @GameId
                  AND image IS NULL;", connection);

            command.Parameters.AddWithValue("@Image", imageBytes);
            command.Parameters.AddWithValue("@GameId", gameId);

            command.ExecuteNonQuery();
        }
    }

    private static string FindProjectRoot(string startPath)
    {
        DirectoryInfo? dir = new DirectoryInfo(startPath);

        while (dir != null)
        {
            bool hasCsproj = dir.GetFiles("*.csproj").Length > 0;
            if (hasCsproj)
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate project root.");
    }
}