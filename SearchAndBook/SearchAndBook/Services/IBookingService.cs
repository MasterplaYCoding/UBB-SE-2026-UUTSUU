using SearchAndBook.Domain;
using SearchAndBook.Shared;

namespace SearchAndBook.Services
{
    internal interface IBookingService
    {
        BookingDTO getGameDetails(int gameId);
        TimeRange[] getUnavailableRanges(int gameId);
        bool checkTimeRange(int gameId, TimeRange range);
    }
}
