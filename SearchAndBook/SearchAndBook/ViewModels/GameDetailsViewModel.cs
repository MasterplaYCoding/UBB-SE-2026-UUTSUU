using SearchAndBook.Domain;
using SearchAndBook.Services;
using SearchAndBook.Shared;

namespace SearchAndBook.ViewModels
{
    internal class GameDetailsViewModel
    {
        private readonly IBookingService bookingService;

        public BookingDTO gameAndUserDetails;

        public TimeRange[] unavailableTimeRanges;

        public GameDetailsViewModel(IBookingService bookingService, int gameId)
        {
            this.bookingService = bookingService;
            gameAndUserDetails = this.bookingService.getGameDetails(gameId);
            unavailableTimeRanges = this.bookingService.getUnavailableRanges(gameId);
        }

        public bool CheckAvailability(TimeRange range)
        {
            return bookingService.checkTimeRange(gameAndUserDetails.gameId, range);
        }

        public int CalculatePrice(TimeRange range)
        {
            int days = (range.endTime - range.startTime).Days;
            if (days == 0)
                days = 1;
            return days * gameAndUserDetails.price;
        }

        public void StartBooking(TimeRange range)
        {
            // later
        }

        public void GoBack()
        {
            // later
        }
    }
}

