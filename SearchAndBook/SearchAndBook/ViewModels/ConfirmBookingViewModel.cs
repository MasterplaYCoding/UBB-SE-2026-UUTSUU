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
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly IBookingService BookingService;

        private BookingDTO _gameAndUserDetails;
        public BookingDTO GameAndUserDetails
        {
            get => _gameAndUserDetails;
            private set { _gameAndUserDetails = value; OnPropertyChanged(); }
        }

        public TimeRange[] UnavailableTimeRanges { get; private set; }

        public TimeRange _selectedTimeRange;
        public TimeRange SelectedTimeRange
        {
            get => _selectedTimeRange;
            private set { _selectedTimeRange = value; 
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
            private set { _totalPrice = value; OnPropertyChanged(); }
        }

        public string StartDate => SelectedTimeRange?.StartTime.ToString("dd MMM yyyy") ?? "-";
        public string EndDate => SelectedTimeRange?.EndTime.ToString("dd MMM yyyy") ?? "-";

        private BitmapImage? _ownerImage;
        public BitmapImage? OwnerImage
        {
            get => _ownerImage;
            private set { _ownerImage = value; OnPropertyChanged(); }
        }

        private BitmapImage? _gameImage;
        public BitmapImage? GameImage
        {
            get => _gameImage;
            private set { _gameImage = value; OnPropertyChanged(); }
        }

        private async void LoadImages()
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

            if (!string.IsNullOrEmpty(GameAndUserDetails.AvatarUrl))
            {
                OwnerImage = new BitmapImage(new Uri(GameAndUserDetails.AvatarUrl));
            }
        }

        public int NumberOfDays
        {
            get
            {
                if (SelectedTimeRange == null) return 1;
                int days = (SelectedTimeRange.EndTime - SelectedTimeRange.StartTime).Days + 1;
                return days == 0 ? 1 : days;
            }
        }

        public event Action OnGoBackRequested;
        public event Action OnConfirmBookingRequested;

        public ConfirmBookingViewModel(IBookingService BookingService, BookingDTO GameAndUserDetails, TimeRange SelectedTimeRange)
        {
            this.BookingService = BookingService;
            this.GameAndUserDetails = GameAndUserDetails;
            this.SelectedTimeRange = SelectedTimeRange;
            UnavailableTimeRanges = this.BookingService.GetUnavailableRanges(GameAndUserDetails.GameId);
            TotalPrice = CalculatePrice();
            LoadImages();
        }

        public bool CheckAvailability(TimeRange range)
        {
            return BookingService.CheckAvailability(GameAndUserDetails.GameId, range);
        }

        public void ConfirmBooking()
        {
            OnConfirmBookingRequested?.Invoke();
        }

        public void GoBack()
        {
            OnGoBackRequested?.Invoke();
        }

        public decimal CalculatePrice()
        {
            int days = (SelectedTimeRange.EndTime - SelectedTimeRange.StartTime).Days + 1;
            if (days == 0) days = 1;
            TotalPrice = days * GameAndUserDetails.Price;
            return TotalPrice;
        }

        public void UpdateSelectedRange(TimeRange newRange)
        {
            SelectedTimeRange = newRange;
            TotalPrice = CalculatePrice();
            OnPropertyChanged(nameof(NumberOfDays));
            OnPropertyChanged(nameof(StartDate));
            OnPropertyChanged(nameof(EndDate));
            OnPropertyChanged(nameof(TotalPrice));
        }
    }
}
