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
        public event Action<BookingDTO, TimeRange>? OnStartBookingRequested;
        public event Action<string>? OnMessageRequested;

        public DateTimeOffset Today => DateTimeOffset.Now.Date;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private BookingDTO _gameAndUserDetails;
        public BookingDTO GameAndUserDetails
        {
            get => _gameAndUserDetails;
            private set
            {
                _gameAndUserDetails = value;
                OnPropertyChanged();
            }
        }

        private bool _hasError;
        public bool HasError
        {
            get => _hasError;
            private set
            {
                _hasError = value;
                OnPropertyChanged();
            }
        }

        private decimal _totalPrice;
        public decimal TotalPrice
        {
            get => _totalPrice;
            private set
            {
                _totalPrice = value;
                OnPropertyChanged();
            }
        }

        private BitmapImage? _gameImage;
        public BitmapImage? GameImage
        {
            get => _gameImage;
            private set
            {
                _gameImage = value;
                OnPropertyChanged();
            }
        }

        private string? _ownerImageUrl;
        public string? OwnerImageUrl
        {
            get => _ownerImageUrl;
            private set
            {
                _ownerImageUrl = value;
                OnPropertyChanged();
            }
        }

        private readonly IBookingService _bookingService;
        public TimeRange[] UnavailableTimeRanges { get; private set; } = Array.Empty<TimeRange>();

        public ICommand GoBackCommand => new RelayCommand(_ => GoBack());

        public ICommand BookCommand => new RelayCommand(obj =>
        {
            try
            {
                if (obj is TimeRange range)
                {
                    StartBooking(range);
                }
                else
                {
                    OnMessageRequested?.Invoke("Invalid booking interval selected.");
                }
            }
            catch (Exception ex)
            {
                OnMessageRequested?.Invoke($"Could not start booking. {ex.Message}");
            }
        });

        public ICommand ChatWithOwnerCommand => new RelayCommand(_ => { /* later */ });

        public GameDetailsViewModel(IBookingService bookingService, int gameId)
        {
            _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));

            try
            {
                GameAndUserDetails = _bookingService.GetGameDetails(gameId);
                UnavailableTimeRanges = _bookingService.GetUnavailableRanges(gameId) ?? Array.Empty<TimeRange>();
                LoadGameImage();
                LoadOwnerImage();
                HasError = false;
            }
            catch (Exception ex)
            {
                HasError = true;
                UnavailableTimeRanges = Array.Empty<TimeRange>();
                OnMessageRequested?.Invoke($"Could not load game details. {ex.Message}");
            }
        }

        public bool CheckAvailability(TimeRange range)
        {
            try
            {
                if (range == null)
                    return false;

                return _bookingService.CheckAvailability(GameAndUserDetails.GameId, range);
            }
            catch (Exception ex)
            {
                OnMessageRequested?.Invoke($"Could not check availability. {ex.Message}");
                return false;
            }
        }

        public decimal CalculatePrice(TimeRange range)
        {
            try
            {
                if (range == null)
                    throw new ArgumentNullException(nameof(range));

                int days = (range.EndTime - range.StartTime).Days + 1;
                if (days <= 0)
                    days = 1;

                TotalPrice = days * GameAndUserDetails.Price;
                return TotalPrice;
            }
            catch (Exception ex)
            {
                OnMessageRequested?.Invoke($"Could not calculate price. {ex.Message}");
                TotalPrice = 0;
                return 0;
            }
        }

        private async void LoadGameImage()
        {
            try
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
            catch (Exception ex)
            {
                GameImage = null;
                OnMessageRequested?.Invoke($"Could not load game image. {ex.Message}");
            }
        }

        private void LoadOwnerImage()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(GameAndUserDetails.AvatarUrl))
                {
                    OwnerImageUrl = null;
                    return;
                }

                OwnerImageUrl = GameAndUserDetails.AvatarUrl;
            }
            catch (Exception ex)
            {
                OwnerImageUrl = null;
                OnMessageRequested?.Invoke($"Could not load owner image. {ex.Message}");
            }
        }

        public void StartBooking(TimeRange range)
        {
            try
            {
                if (SessionContext.GetInstance().UserId == -1)
                {
                    OnMessageRequested?.Invoke("User not logged in. Please log in first");
                    return;
                }

                if (range == null)
                {
                    OnMessageRequested?.Invoke("Please select a valid booking range.");
                    return;
                }

                OnStartBookingRequested?.Invoke(GameAndUserDetails, range);
            }
            catch (Exception ex)
            {
                OnMessageRequested?.Invoke($"Could not continue to booking. {ex.Message}");
            }
        }

        public void GoBack()
        {
            try
            {
                OnGoBackRequested?.Invoke();
            }
            catch (Exception ex)
            {
                OnMessageRequested?.Invoke($"Could not go back. {ex.Message}");
            }
        }
    }
}