using System;
using SearchAndBook.Domain;
using SearchAndBook.Repositories;
using SearchAndBook.Shared;

namespace SearchAndBook.Services;

/// <summary>
/// Service responsible for handling booking operations, including retrieving game details,
/// checking availability, and managing rental time ranges.
/// </summary>
public class BookingService : IBookingService
{
    private readonly IGamesRepository gamesRepo;
    private readonly IRentalsRepository rentalsRepo;
    private readonly IUsersRepository usersRepo;

    /// <summary>
    /// Initializes a new instance of the BookingService class.
    /// </summary>
    /// <param name="gamesRepo">The games repository.</param>
    /// <param name="rentalsRepo">The rentals repository.</param>
    /// <param name="usersRepo">The users repository.</param>
    public BookingService(
        IGamesRepository gamesRepo,
        IRentalsRepository rentalsRepo,
        IUsersRepository usersRepo)
    {
        this.gamesRepo = gamesRepo;
        this.rentalsRepo = rentalsRepo;
        this.usersRepo = usersRepo;
    }

    /// <summary>
    /// Retrieves detailed booking information for a specific game, including owner details.
    /// </summary>
    /// <param name="gameId">The unique identifier of the game.</param>
    /// <returns>A <see cref="BookingDTO"/> containing the game and owner details.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the game or its owner cannot be found.</exception>
    public BookingDTO GetGameDetails(int gameId)
    {
        try
        {
            var game = gamesRepo.Get(gameId);
            if (game == null)
            {
                throw new InvalidOperationException($"Game with id {gameId} was not found.");
            }

            var owner = usersRepo.Get(game.OwnerId);
            if (owner == null)
            {
                throw new InvalidOperationException($"Owner for game id {gameId} was not found.");
            }

            return new BookingDTO
            {
                GameId = game.GameId,
                Name = game.Name,
                Image = game.Image,
                Price = game.Price,
                City = owner.City,
                MinimumNrPlayers = game.MinimumPlayerNumber,
                MaximumNrPlayers = game.MaximumPlayerNumber,
                Description = game.Description,
                UserId = owner.UserId,
                DisplayName = owner.DisplayName,
                IsSuspended = owner.IsSuspended,
                AvatarUrl = owner.AvatarUrl,
                CreatedAt = owner.CreatedAt
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve details for game {gameId}.", ex);

        }
    }

    /// <summary>
    /// Retrieves all the time ranges during which a specific game is unavailable.
    /// </summary>
    /// <param name="gameId">The unique identifier of the game.</param>
    /// <returns>An array of <see cref="TimeRange"/> representing the unavailable periods.</returns>
    public TimeRange[] GetUnavailableRanges(int gameId)
    {
        try
        {
            return rentalsRepo
                .GetUnavailableRanges(gameId)
                .ToArray();
        } catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve unavailable time ranges for game {gameId}.", ex);
        }
    }

    /// <summary>
    /// Checks whether a specific game is available during the given time range.
    /// </summary>
    /// <param name="gameId">The unique identifier of the game.</param>
    /// <param name="range">The requested <see cref="TimeRange"/> for the booking.</param>
    /// <returns><c>true</c> if the game is available for the specified range; otherwise, <c>false</c>.</returns>
    public bool CheckAvailability(int gameId, TimeRange range)
    {
        try
        {
            return rentalsRepo.CheckAvailability(range, gameId);
        } catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to check availability for game {gameId}.", ex);
        }
    }
}