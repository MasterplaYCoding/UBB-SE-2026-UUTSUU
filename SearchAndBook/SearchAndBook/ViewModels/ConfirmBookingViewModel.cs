using Microsoft.UI.Xaml.Media.Imaging;
using SearchAndBook.Domain;
using SearchAndBook.Services;
using SearchAndBook.Shared;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;

namespace SearchAndBook.ViewModels
{
    internal class ConfirmBookingViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action<string>? OnErrorOccurred;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly IBookingService BookingService;

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

        public TimeRange[] UnavailableTimeRanges { get; private set; } = Array.Empty<TimeRange>();

        private TimeRange _selectedTimeRange;
        public TimeRange SelectedTimeRange
        {
            get => _selectedTimeRange;
            private set
            {
                _selectedTimeRange = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NumberOfDays));
                OnPropertyChanged(nameof(StartDate));
                OnPropertyChanged(nameof(EndDate));
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

        public string StartDate => SelectedTimeRange?.StartTime.ToString("dd MMM yyyy") ?? "-";
        public string EndDate => SelectedTimeRange?.EndTime.ToString("dd MMM yyyy") ?? "-";

        private BitmapImage? _ownerImage;
        public BitmapImage? OwnerImage
        {
            get => _ownerImage;
            private set
            {
                _ownerImage = value;
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

        public event Action? OnGoBackRequested;
        public event Action? OnConfirmBookingRequested;

        public ConfirmBookingViewModel(IBookingService bookingService, BookingDTO gameAndUserDetails, TimeRange selectedTimeRange)
        {
            try
            {
                BookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
                GameAndUserDetails = gameAndUserDetails ?? throw new ArgumentNullException(nameof(gameAndUserDetails));
                SelectedTimeRange = selectedTimeRange ?? throw new ArgumentNullException(nameof(selectedTimeRange));

                UnavailableTimeRanges = BookingService.GetUnavailableRanges(GameAndUserDetails.GameId) ?? Array.Empty<TimeRange>();
                TotalPrice = CalculatePrice();
                LoadImages();
            }
            catch (Exception ex)
            {
                RaiseError($"Could not initialize booking confirmation. {ex.Message}");
                UnavailableTimeRanges = Array.Empty<TimeRange>();
                TotalPrice = 0;
            }
        }

        private async void LoadImages()
        {
            try
            {
                if (GameAndUserDetails.Image != null && GameAndUserDetails.Image.Length > 0)
                {
                    using var stream = new InMemoryRandomAccessStream();
                    await stream.WriteAsync(GameAndUserDetails.Image.AsBuffer());
                    stream.Seek(0);
                    var bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(stream);
                    GameImage = bitmap;
                }
                else
                {
                    GameImage = null;
                }

                if (!string.IsNullOrEmpty(GameAndUserDetails.AvatarUrl))
                {
                    OwnerImage = new BitmapImage(new Uri(GameAndUserDetails.AvatarUrl));
                }
                else
                {
                    OwnerImage = null;
                }
            }
            catch (Exception ex)
            {
                GameImage = null;
                OwnerImage = null;
                RaiseError($"Could not load images. {ex.Message}");
            }
        }

        public int NumberOfDays
        {
            get
            {
                try
                {
                    if (SelectedTimeRange == null)
                        return 1;

                    int days = (SelectedTimeRange.EndTime - SelectedTimeRange.StartTime).Days + 1;
                    return days <= 0 ? 1 : days;
                }
                catch
                {
                    return 1;
                }
            }
        }

        public bool CheckAvailability(TimeRange range)
        {
            try
            {
                if (range == null)
                    return false;

                return BookingService.CheckAvailability(GameAndUserDetails.GameId, range);
            }
            catch (Exception ex)
            {
                RaiseError($"Could not check availability. {ex.Message}");
                return false;
            }
        }

        public void ConfirmBooking()
        {
            try
            {
                OnConfirmBookingRequested?.Invoke();
            }
            catch (Exception ex)
            {
                RaiseError($"Could not confirm booking. {ex.Message}");
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
                RaiseError($"Could not go back. {ex.Message}");
            }
        }

        public decimal CalculatePrice()
        {
            try
            {
                int days = (SelectedTimeRange.EndTime - SelectedTimeRange.StartTime).Days + 1;
                if (days <= 0) days = 1;

                TotalPrice = days * GameAndUserDetails.Price;
                return TotalPrice;
            }
            catch (Exception ex)
            {
                RaiseError($"Could not calculate price. {ex.Message}");
                TotalPrice = 0;
                return 0;
            }
        }

        public void UpdateSelectedRange(TimeRange newRange)
        {
            try
            {
                if (newRange == null)
                    throw new ArgumentNullException(nameof(newRange));

                SelectedTimeRange = newRange;
                TotalPrice = CalculatePrice();
                OnPropertyChanged(nameof(NumberOfDays));
                OnPropertyChanged(nameof(StartDate));
                OnPropertyChanged(nameof(EndDate));
                OnPropertyChanged(nameof(TotalPrice));
            }
            catch (Exception ex)
            {
                RaiseError($"Could not update selected range. {ex.Message}");
            }
        }

        private void RaiseError(string message)
        {
            OnErrorOccurred?.Invoke(message);
        }
    }
}