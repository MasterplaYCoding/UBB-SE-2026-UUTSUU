using System;
using SearchAndBook.Domain;
using SearchAndBook.Shared;
using SearchAndBook.Repositories;
 
namespace SearchAndBook.Services
{
    internal class BookingService : IBookingService
    {
        private readonly IGameRepository gamesRepo;
        private readonly IRentalRepository rentalsRepo;
        private readonly IUserRepository usersRepo;

        public BookingService(IGameRepository gamesRepo, IRentalRepository rentalsRepo, IUserRepository usersRepo)
        {
            this.gamesRepo = gamesRepo;
            this.rentalsRepo = rentalsRepo;
            this.usersRepo = usersRepo;
        }
        public BookingDTO getGameDetails(int gameId)
        {
            /*var game = gamesRepo.get(gameId); // IRepo method - overriden by gameRepo
            if (game == null)
                return null;
            var user = usersRepo.get(game.ownerId); // IRepo method - overriden by userRepo

            return new BookingDTO
            {
                gameId = game.gameId,
                name = game.name,
                image = game.image,
                price = game.price,
                minimumNrPlayers = game.minimumPlayerNumber,
                maximumNrPlayers = game.maximumPlayerNumber,
                userId = user.userId,
                displayName = user.displayName,
                isSuspended = user.isSuspended,
                avatarURL = user.avatarURL,
                createdAt = user.createdAt,
                city = game.city,
                ownerCity = user.city
            }; */
            return new BookingDTO { };
        }

        public TimeRange[] getUnavailableRanges(int gameId)
        {
            //return rentalsRepo.getUnavailableRanges(gameId); // needs to be implemented in RentalRepo
            return new TimeRange[0];
        }

        public bool checkTimeRange(int gameId, TimeRange range)
        {
            /*var unavailableRanges = rentalsRepo.getUnavailableRanges(gameId);

            foreach (var booked in unavailableRanges)
            {
                bool overlap = range.startTime < booked.endTime && range.endTime > booked.startTime;
                if (overlap)
                    return false;
            }*/
            return true;
        }
    }
}
