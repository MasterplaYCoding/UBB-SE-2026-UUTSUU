using SearchAndBook.Domain;
using SearchAndBook.Shared;

namespace SearchAndBook.Services
{
    /// <summary>
    /// Defines the operations for managing and querying game bookings.
    /// </summary>
    public interface IBookingService
    {
        /// <summary>
        /// Retrieves the booking details for a specific game.
        /// </summary>
        /// <param name="gameId">The unique identifier of the game.</param>
        /// <returns>A <see cref="BookingDTO"/> containing the game details.</returns>
        BookingDTO GetGameDetails(int gameId);

        /// <summary>
        /// Retrieves all unavailable time ranges for a specific game.
        /// </summary>
        /// <param name="gameId">The unique identifier of the game.</param>
        /// <returns>An array of <see cref="TimeRange"/> representing periods when the game is unavailable.</returns>
        TimeRange[] GetUnavailableRanges(int gameId);

        /// <summary>
        /// Checks if a game is available for booking during a specified time range.
        /// </summary>
        /// <param name="gameId">The unique identifier of the game.</param>
        /// <param name="range">The time range to check for availability.</param>
        /// <returns><c>true</c> if the game is available during the specified range; otherwise, <c>false</c>.</returns>
        bool CheckAvailability(int gameId, TimeRange range);
    }
}
