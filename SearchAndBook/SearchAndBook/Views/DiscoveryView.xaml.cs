using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SearchAndBook.Domain;
using SearchAndBook.Repositories;
using SearchAndBook.Services;
using SearchAndBook.Shared;
using SearchAndBook.ViewModels;

namespace SearchAndBook.Views
{
    public sealed partial class DiscoveryView : Page
    {
        public DiscoveryViewModel ViewModel { get; private set; }

        public DiscoveryView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var gamesRepository = new GamesRepository();
            var usersRepository = new UsersRepository();
            var rentalsRepository = new RentalsRepository();
            var geoService = App.GlobalGeoService!;

            var service = new SearchAndFilterService(
                gamesRepository,
                usersRepository,
                rentalsRepository,
                geoService);

            ViewModel = new DiscoveryViewModel(service, geoService);
            ViewModel.OnGameSelectedRequest += gameId =>
            {
                Frame.Navigate(typeof(GameDetailsView), gameId);
            };

            DataContext = ViewModel;
          
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            var criteria = new FilterCriteria
            {
                Name = GameTextBox.Text,
                City = ViewModel.CitySearchText
            };

            if (StartDatePicker.Date != null && EndDatePicker.Date != null)
            {
                criteria.AvailabilityRange = new TimeRange(
                    StartDatePicker.Date.Value.DateTime,
                    EndDatePicker.Date.Value.DateTime);
            }

            Frame.Navigate(typeof(FilteredSearchView), criteria);
        }

        private void Game_Click(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is GameDTO game)
            {
                Frame.Navigate(typeof(GameDetailsView), game.GameId);
            }
        }
    }
}