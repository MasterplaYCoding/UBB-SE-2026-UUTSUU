using System;
using System.Collections.Generic;
using System.Linq;
using SearchAndBook.Repositories;
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
        private readonly IRentalsRepository rentalsRepository;

        public SearchAndFilterService(IGamesRepository gamesRepository, IUsersRepository usersRepository, IRentalsRepository rentalsRepository)
        {
            this.gamesRepository = gamesRepository;
            this.usersRepository = usersRepository;
            this.rentalsRepository = rentalsRepository;
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

        public GameDTO[] ApplyFilters(GameDTO[] sourceGames, FilterCriteria filter)
        {
            IEnumerable<GameDTO> filteredGames = sourceGames;

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                filteredGames = filteredGames.Where(game => game.Name.Contains(filter.Name, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.City))
            {
                filteredGames = filteredGames.Where(game => game.City.Contains(filter.City, StringComparison.OrdinalIgnoreCase));
            }


            if (filter.MaximumPrice.HasValue)
            {
                filteredGames = filteredGames.Where(game => game.Price <= filter.MaximumPrice.Value);
            }

            if (filter.PlayerCount.HasValue)
            {
                filteredGames = filteredGames.Where(game => game.MaximumPlayerNumber >= filter.PlayerCount.Value);
            }

            switch (filter.SortOption)
            {
                case SortOption.PriceAscending:
                    filteredGames = filteredGames.OrderBy(game => game.Price);
                    break;
                case SortOption.PriceDescending:
                    filteredGames = filteredGames.OrderByDescending(game => game.Price);
                    break;
                case SortOption.None:
                default:
                    break;
            }


            if (filter.AvailabilityRange != null)
            {

                filteredGames = filteredGames.Where(game =>
                    rentalsRepository.CheckAvailability(filter.AvailabilityRange, game.GameId));
            }
            return filteredGames.ToArray();

        }
    }
}
