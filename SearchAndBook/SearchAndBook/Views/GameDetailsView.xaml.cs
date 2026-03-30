using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SearchAndBook.Domain;
using SearchAndBook.Repositories;
using SearchAndBook.Services;
using SearchAndBook.ViewModels;

namespace SearchAndBook.Views
{
    public sealed partial class GameDetailsView : Page
    {
        public GameDetailsView()
        {
            InitializeComponent();
            var gameRepo = new GameRepository();
            var rentalRepo = new RentalRepository();
            var userRepo = new UserRepository();
            var service = new BookingService(gameRepo, rentalRepo, userRepo);
            var vm = new GameDetailsViewModel(service, 1);

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

        private void OnBookClicked(object sender, RoutedEventArgs e)
        {
            var vm = (GameDetailsViewModel)this.DataContext;

            var selectedDates = RentalCalendar.SelectedDates;
            if (selectedDates.Count < 2)
                return;

            var startTime = selectedDates[0].DateTime;
            var endTime = selectedDates[selectedDates.Count - 1].DateTime;

            var range = new TimeRange { startTime = startTime, endTime = endTime };

            vm.StartBooking(range);
        }
    }
}