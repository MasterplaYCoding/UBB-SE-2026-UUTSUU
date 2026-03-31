using SearchAndBook.Repositories;
using System.Linq;
using SearchAndBook.Shared;

namespace SearchAndBook.Services
{
    /// <summary>
    /// Service responsible for searching, filtering, and retrieving game feeds.
    /// </summary>
    internal class SearchAndFilterService : ISearchAndFilterService
    {
        private readonly IGamesRepository gamesRepository;
        private readonly IUsersRepository usersRepository;

        public SearchAndFilterService(IGamesRepository gamesRepository, IUsersRepository usersRepository)
        {
            this.gamesRepository = gamesRepository;
            this.usersRepository = usersRepository;
        }

        /// <summary>
        /// Searches for games based on the provided filter criteria.
        /// </summary>
        /// <param name="filter">The criteria to filter games.</param>
        /// <returns>An array of <see cref="GameDTO"/> matching the criteria.</returns>
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

        /// <summary>
        /// Retrieves a feed of games available tonight for the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user requesting the feed or null.</param>
        /// <returns>An array of <see cref="GameDTO"/> available tonight.</returns>
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

        /// <summary>
        /// Retrieves a feed of other relevant games for the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user requesting the feed or null.</param>
        /// <returns>An array of <see cref="GameDTO"/> representing other games.</returns>
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
