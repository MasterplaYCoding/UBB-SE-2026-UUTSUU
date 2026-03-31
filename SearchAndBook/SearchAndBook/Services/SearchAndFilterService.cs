using SearchAndBook.Repositories;
using SearchAndBook.Shared;
using SearchAndBook.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

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

        private readonly IGeoService _geoService;

        public SearchAndFilterService(IGamesRepository gamesRepository, IUsersRepository usersRepository, IRentalsRepository rentalsRepository, IGeoService geoService)
        {
            this.gamesRepository = gamesRepository;
            this.usersRepository = usersRepository;
            this.rentalsRepository = rentalsRepository;
            this._geoService = geoService;
        }

        /// <summary>
        /// Searches for games based on the provided filter criteria.
        /// </summary>
        /// <param name="filter">The criteria to filter games.</param>
        /// <returns>An array of <see cref="GameDTO"/> matching the criteria.</returns>
        public GameDTO[] Search(FilterCriteria filter)
        {
            string? originalCity = filter.City;
            if (filter.SortOption == SortOption.Location)
            {
                filter.City = null;
            }

            var games = gamesRepository.GetByFilter(filter);
            filter.City = originalCity;

            GameDTO[] result = games.Select(g =>
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

            //// sorting by distance
            
            //// this is if we decide to only use this methode and remove the ApplyFilters method
            //// only runs this code if SortOption is set, so never from feed


            return ApplyFilters(result, filter);
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

            if (filter.MaximumPrice.HasValue)
            {
                filteredGames = filteredGames.Where(game => game.Price <= filter.MaximumPrice.Value);
            }

            if (filter.PlayerCount.HasValue)
            {
                filteredGames = filteredGames.Where(game => game.MaximumPlayerNumber >= filter.PlayerCount.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.City) && filter.SortOption != SortOption.Location)
            {
                filteredGames = filteredGames.Where(game =>
                    !string.IsNullOrWhiteSpace(game.City) &&
                    game.City.Contains(filter.City, StringComparison.OrdinalIgnoreCase));
            }


            switch (filter.SortOption)
            {
                case SortOption.PriceAscending:
                    filteredGames = filteredGames.OrderBy(game => game.Price);
                    break;

                case SortOption.PriceDescending:
                    filteredGames = filteredGames.OrderByDescending(game => game.Price);
                    break;

                case SortOption.Location:
                    if (!string.IsNullOrWhiteSpace(filter.City))
                    {
                        var userCity = _geoService.GetCityDetails(filter.City);
                        if (userCity.found)
                        {
                            var distanceCache = new Dictionary<string, double?>();

                            filteredGames = filteredGames.OrderBy(g =>
                            {
                                if (string.IsNullOrWhiteSpace(g.City)) return double.MaxValue;

                                if (!distanceCache.TryGetValue(g.City, out double? distance))
                                {
                                    var gameCity = _geoService.GetCityDetails(g.City);
                                    distance = gameCity.found
                                        ? GeographicDistance.CalculateDistance(userCity.lat, userCity.lon, gameCity.lat, gameCity.lon)
                                        : null;

                                    distanceCache[g.City] = distance;
                                }

                                return distance ?? double.MaxValue;
                            });
                        }
                    }
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
