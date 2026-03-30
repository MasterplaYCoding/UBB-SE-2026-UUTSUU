using SearchAndBook.Domain;
using SearchAndBook.Shared;

namespace SearchAndBook.Services
{
    public interface IBookingService
    {
        BookingDTO GetGameDetails(int gameId);
        TimeRange[] GetUnavailableRanges(int gameId);
        bool CheckAvailability(int gameId, TimeRange range);
    }
}
