using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SearchAndBook.Domain;
using SearchAndBook.Repositories;
using SearchAndBook.Services;
using SearchAndBook.ViewModels;
using System;

namespace SearchAndBook.Views
{
    public sealed partial class GameDetailsView : Page
    {
        public GameDetailsView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is not int gameId) { return; }

            var gameRepo = new GamesRepository();
            var rentalRepo = new RentalsRepository();
            var userRepo = new UsersRepository();
            var service = new BookingService(gameRepo, rentalRepo, userRepo);
            var vm = new GameDetailsViewModel(service, gameId);

            vm.OnGoBackRequested += () =>
            {
                if (Frame.CanGoBack)
                    Frame.GoBack();
            };

            vm.OnStartBookingRequested += () =>
            {
                Frame.Navigate(typeof(ConfirmBookingView));
            };

            this.DataContext = vm;
        }

        private void OnBackClicked(object sender, RoutedEventArgs e)
        {
            var vm = (GameDetailsViewModel)this.DataContext;
            vm.GoBack();
        }

        private async void OnBookClicked(object sender, RoutedEventArgs e)
        {
            var selectedDates = RentalCalendar.SelectedDates;
            if (selectedDates.Count == 0)
            {
                var dialog = new ContentDialog
                {
                    Title = "Invalid selection",
                    Content = "Please select at least one date.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            var vm = (GameDetailsViewModel)this.DataContext;
            var range = new TimeRange(selectedDates[0].DateTime, selectedDates[selectedDates.Count - 1].DateTime);
            vm.StartBooking(range);
        }

        private void OnDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs e)
        {
            var vm = (GameDetailsViewModel)this.DataContext;
            var selectedDates = RentalCalendar.SelectedDates;
            if (selectedDates.Count < 1)
                return;

            var range = new TimeRange(selectedDates[0].DateTime, selectedDates[selectedDates.Count - 1].DateTime);

            vm.CalculatePrice(range);
        }
    }
}