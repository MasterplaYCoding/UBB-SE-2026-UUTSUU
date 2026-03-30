using SearchAndBook.Repositories;
using System.Linq;
using SearchAndBook.Shared;

namespace SearchAndBook.Services
{
    internal class SearchAndFilterService : ISearchAndFilterService
    {
        private readonly IGamesRepository gamesRepository;
        private readonly IUsersRepository usersRepository;
        public SearchAndFilterService(IGamesRepository gamesRepository, IUsersRepository usersRepository)
        {
            this.gamesRepository = gamesRepository;
            this.usersRepository = usersRepository;
        }
        public GameDTO[] Search(FilterCriteria filter)
        {
            var games = gamesRepository.GetByFilter(filter);

            return games.Select(g =>
            {
                var owner = usersRepository.Get(g.OwnerId);

                return new GameDTO
                {
                    GameId = g.GameId,
                    Name = g.Name,
                    Image = g.Image,
                    Price = g.Price,
                    City = owner?.City ?? string.Empty,
                    MaximumPlayerNumber = g.MaximumPlayerNumber,
                    MinimumPlayerNumber = g.MinimumPlayerNumber
                };
            }).ToArray();
        }
        public GameDTO[] GetFeedAvailableTonight(int userId)
        {
            var games = gamesRepository.GetForFeedAvailableTonight(userId);

            return games.Select(g =>
            {
                var owner = usersRepository.Get(g.OwnerId);

                return new GameDTO
                {
                    GameId = g.GameId,
                    Name = g.Name,
                    Image = g.Image,
                    Price = g.Price,
                    City = owner?.City ?? string.Empty,
                    MinimumPlayerNumber = g.MinimumPlayerNumber,
                    MaximumPlayerNumber = g.MaximumPlayerNumber
                };
            }).ToArray();
        }

        public GameDTO[] GetFeedOthers(int userId)
        {
            var games = gamesRepository.GetForFeedOthers(userId);

            return games.Select(g =>
            {
                var owner = usersRepository.Get(g.OwnerId);

                return new GameDTO
                {
                    GameId = g.GameId,
                    Name = g.Name,
                    Image = g.Image,
                    Price = g.Price,
                    City = owner?.City ?? string.Empty,
                    MinimumPlayerNumber = g.MinimumPlayerNumber,
                    MaximumPlayerNumber = g.MaximumPlayerNumber
                };
            }).ToArray();
        }
    }
}
