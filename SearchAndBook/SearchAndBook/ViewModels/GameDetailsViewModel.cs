using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using SearchAndBook.Domain;
using SearchAndBook.Services;
using SearchAndBook.Shared;
using Windows.Storage.Streams;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SearchAndBook.ViewModels
{
    public class GameDetailsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly IBookingService bookingService;
        public BookingDTO GameAndUserDetails { get; private set; }

        public TimeRange[] UnavailableTimeRanges { get; private set; }

        private BitmapImage? gameImage;
        public BitmapImage? GameImage
        {
            get => gameImage;
            private set
            {
                gameImage = value;
                OnPropertyChanged();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public GameDetailsViewModel(IBookingService bookingService, int gameId)
        {
            this.bookingService = bookingService;
            GameAndUserDetails = this.bookingService.GetGameDetails(gameId);
            LoadGameImage();
            UnavailableTimeRanges = this.bookingService.GetUnavailableRanges(gameId);
        }

        public bool CheckAvailability(TimeRange range)
        {
            return bookingService.CheckAvailability(GameAndUserDetails.GameId, range);
        }

        public decimal CalculatePrice(TimeRange range)
        {
            int days = (range.EndTime - range.StartTime).Days;
            if (days == 0)
                days = 1;
            return days * GameAndUserDetails.Price;
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

