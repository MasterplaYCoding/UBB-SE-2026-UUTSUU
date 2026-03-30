using Microsoft.UI.Xaml.Controls;
using SearchAndBook.Domain;
using SearchAndBook.Services;
using SearchAndBook.Shared;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SearchAndBook.ViewModels
{
    internal class GameDetailsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private BookingDTO _gameAndUserDetails;
        public BookingDTO gameAndUserDetails
        {
            get => _gameAndUserDetails;
            private set { _gameAndUserDetails = value; OnPropertyChanged(); }
        }

        private bool _hasError;
        public bool hasError
        {
            get => _hasError;
            private set { _hasError = value; OnPropertyChanged(); }
        }

        private int _totalPrice;
        public int totalPrice
        {
            get => _totalPrice;
            private set { _totalPrice = value; OnPropertyChanged(); }
        }

        private readonly IBookingService bookingService;
        public TimeRange[] unavailableTimeRanges;

        public event Action OnGoBackRequested;
        public event Action OnStartBookingRequested;

        public GameDetailsViewModel(IBookingService bookingService, int gameId)
        {
            this.bookingService = bookingService;
            try
            {
                gameAndUserDetails = this.bookingService.getGameDetails(gameId);
                unavailableTimeRanges = this.bookingService.getUnavailableRanges(gameId);
                hasError = false;
            }
            catch
            {
                hasError = true;
            }
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
            totalPrice = days * gameAndUserDetails.price;
            return totalPrice;
        }

        public void StartBooking(TimeRange range)
        {
            OnStartBookingRequested?.Invoke();
        }

        public void GoBack()
        {
            OnGoBackRequested?.Invoke();
        }
    }
}

