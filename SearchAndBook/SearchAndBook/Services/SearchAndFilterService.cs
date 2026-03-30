using SearchAndBook.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SearchAndBook.Shared;

namespace SearchAndBook.Services
{
    internal class SearchAndFilterService : ISearchAndFilterService
    {
        private readonly IGameRepository gameRepository;
        private readonly IRentalRepository rentalRepository;
        public SearchAndFilterService(IGameRepository gameRepository, IRentalRepository rentalRepository)
        {
            this.gameRepository = gameRepository;
            this.rentalRepository = rentalRepository;
        }
        public List<GameDTO> search(FilterCriteria filter)
        {
            var games = gameRepository.getByFilter(filter);

            return games.Select(g => new GameDTO
            {
                GameId = g.GameId,
                Name = g.Name,
                Image = g.Image,
                Price = g.Price,
                City = g.City,
                MaximumPlayerNumber = g.MaximumPlayerNumber,
                MininumPlayerNumber = g.MininumPlayerNumber
            }).ToList();
        }
        public List<GameDTO> getFeedAvailableTonight(int userId)
        {
            // Implement logic to get games available tonight based on the name using gameRepository and rentalRepository
            // This is a placeholder implementation and should be replaced with actual logic
            return new List<GameDTO>();
        }
        public List<GameDTO> getFeedOthers(int userId)
        {
            // Implement logic to get other games based on the name using gameRepository and rentalRepository
            // This is a placeholder implementation and should be replaced with actual logic
            return new List<GameDTO>();
        }
    }
}
