using System;
using SearchAndBook.Domain;
using SearchAndBook.Repositories;
using SearchAndBook.Shared;

namespace SearchAndBook.Services;

public class BookingService : IBookingService
{
    private readonly IGamesRepository gamesRepo;
    private readonly IRentalsRepository rentalsRepo;
    private readonly IUsersRepository usersRepo;

    public BookingService(
        IGamesRepository gamesRepo,
        IRentalsRepository rentalsRepo,
        IUsersRepository usersRepo)
    {
        this.gamesRepo = gamesRepo;
        this.rentalsRepo = rentalsRepo;
        this.usersRepo = usersRepo;
    }

    public BookingDTO GetGameDetails(int gameId)
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

    public TimeRange[] GetUnavailableRanges(int gameId)
    {
        return rentalsRepo
            .GetUnavailableRanges(gameId)
            .ToArray();
    }

    public bool CheckAvailability(int gameId, TimeRange range)
    {
        return rentalsRepo.CheckAvailability(range, gameId);
    }
}