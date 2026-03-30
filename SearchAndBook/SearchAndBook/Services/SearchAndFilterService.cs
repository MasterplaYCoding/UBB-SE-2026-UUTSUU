using SearchAndBook.Domain;
using SearchAndBook.Repositories;
using SearchAndBook.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchAndBook.Services
{
    public class SearchAndFilterService : ISearchAndFilterService
    {
        private readonly IGamesRepository _gameRepository;
        private readonly IRentalsRepository _rentalRepository;
        private readonly IUsersRepository _userRepository;
        public SearchAndFilterService(IGamesRepository gameRepository, IRentalsRepository rentalRepository, IUsersRepository userRepository)
        {
            this._gameRepository = gameRepository;
            this._rentalRepository = rentalRepository;
            this._userRepository = userRepository;
        }
        public GameDTO[] Search(FilterCriteria filter)
        {
            var games = _gameRepository.GetByFilter(filter);

            return games.Select(MapGameToDto).ToArray();
        }
        public GameDTO[] GetFeedAvailableTonight(int userId)
        {
            // Implement logic to get games available tonight based on the name using gameRepository and rentalRepository
            // This is a placeholder implementation and should be replaced with actual logic
            return Array.Empty<GameDTO>();
        }
        public GameDTO[] GetFeedOthers(int userId)
        {
            // Implement logic to get other games based on the name using gameRepository and rentalRepository
            // This is a placeholder implementation and should be replaced with actual logic
            return Array.Empty<GameDTO>();
        }

        public GameDTO[] ApplyFilters(GameDTO[] sourceGames, FilterCriteria filter){
            IEnumerable<GameDTO> filteredGames = sourceGames;

            if (!string.IsNullOrWhiteSpace(filter.Name)) {
                filteredGames = filteredGames.Where(game => game.Name.Contains(filter.Name, StringComparison.OrdinalIgnoreCase));  
            }

            if (!string.IsNullOrWhiteSpace(filter.City)) {
                filteredGames = filteredGames.Where(game => game.City.Contains(filter.City, StringComparison.OrdinalIgnoreCase));  
            }


            if (filter.MaximumPrice.HasValue) { 
                filteredGames = filteredGames.Where(game => game.Price <= filter.MaximumPrice.Value);
            }

            if (filter.PlayerCount.HasValue) { 
                filteredGames = filteredGames.Where(game => game.MaximumPlayerNumber >= filter.PlayerCount.Value);
            }

            switch (filter.SortOption){
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
                    _rentalRepository.CheckAvailability(filter.AvailabilityRange, game.GameId));
            }



            return filteredGames.ToArray();


        }


        private GameDTO MapGameToDto(Game game)
        {
            var owner = _userRepository.Get(game.OwnerId);

            return new GameDTO
            {
                GameId = game.GameId,
                Name = game.Name,
                Image = game.Image,
                Price = game.Price,
                City = owner?.City ?? string.Empty,
                MaximumPlayerNumber = game.MaximumPlayerNumber,
                MinimumPlayerNumber = game.MinimumPlayerNumber
            };
        }
    }
}
