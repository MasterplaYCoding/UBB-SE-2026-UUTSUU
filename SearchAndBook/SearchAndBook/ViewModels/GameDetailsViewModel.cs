using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using SearchAndBook.CommandHandler;
using SearchAndBook.Domain;
using SearchAndBook.Services;
using SearchAndBook.Shared;
using Windows.Storage.Streams;

namespace SearchAndBook.ViewModels
{
    public class GameDetailsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action? OnGoBackRequested;
        public event Action<BookingDTO, TimeRange> OnStartBookingRequested;
        public event Action<string>? OnMessageRequested;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private BookingDTO _gameAndUserDetails;
        public BookingDTO GameAndUserDetails
        {
            get => _gameAndUserDetails;
            private set { _gameAndUserDetails = value; OnPropertyChanged(); }
        }

        private bool _hasError;
        public bool HasError
        {
            get => _hasError;
            private set { _hasError = value; OnPropertyChanged(); }
        }

        private decimal _totalPrice;
        public decimal TotalPrice
        {
            get => _totalPrice;
            private set { _totalPrice = value; OnPropertyChanged(); }
        }

        private BitmapImage? _gameImage;
        public BitmapImage? GameImage
        {
            get => _gameImage;
            private set { _gameImage = value; OnPropertyChanged(); }
        }

        private string? _ownerImageUrl;
        public string? OwnerImageUrl
        {
            get => _ownerImageUrl;
            private set { _ownerImageUrl = value; OnPropertyChanged(); }
        }

        private readonly IBookingService _bookingService;
        public TimeRange[] UnavailableTimeRanges { get; private set; }

        public ICommand GoBackCommand => new RelayCommand(_ => GoBack());
        public ICommand BookCommand => new RelayCommand(obj =>
        {
            if (obj is TimeRange range)
            {
                StartBooking(range);
            }
        });
        public ICommand ChatWithOwnerCommand => new RelayCommand(_ => { /* later */ });

        public GameDetailsViewModel(IBookingService bookingService, int gameId)
        {
            _bookingService = bookingService;
            try
            {
                GameAndUserDetails = _bookingService.GetGameDetails(gameId);
                UnavailableTimeRanges = _bookingService.GetUnavailableRanges(gameId);
                LoadGameImage();
                LoadOwnerImage();
                HasError = false;
            }
            catch
            {
                HasError = true;
            }
        }

        public bool CheckAvailability(TimeRange range)
        {
            return _bookingService.CheckAvailability(GameAndUserDetails.GameId, range);
        }

        public decimal CalculatePrice(TimeRange range)
        {
            int days = (range.EndTime - range.StartTime).Days + 1;
            if (days == 0)
                days = 1;

            TotalPrice = days * GameAndUserDetails.Price;
            return TotalPrice;
        }

        private async void LoadGameImage()
        {
            if (GameAndUserDetails.Image == null || GameAndUserDetails.Image.Length == 0)
            {
                GameImage = null;
                return;
            }

            using var stream = new InMemoryRandomAccessStream();
            await stream.WriteAsync(GameAndUserDetails.Image.AsBuffer());
            stream.Seek(0);

            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(stream);
            GameImage = bitmap;
        }

        private void LoadOwnerImage()
        {
            if (string.IsNullOrWhiteSpace(GameAndUserDetails.AvatarUrl))
            {
                OwnerImageUrl = null;
                return;
            }

            OwnerImageUrl = GameAndUserDetails.AvatarUrl;
        }

        public void StartBooking(TimeRange range)
        {
            if (SessionContext.GetInstance().UserId == -1)
            {
                OnMessageRequested?.Invoke("User not logged in. Please log in first");
                return;
            }

            OnStartBookingRequested?.Invoke(GameAndUserDetails, range);
        }

        public void GoBack()
        {
            OnGoBackRequested?.Invoke();
        }
    }
}