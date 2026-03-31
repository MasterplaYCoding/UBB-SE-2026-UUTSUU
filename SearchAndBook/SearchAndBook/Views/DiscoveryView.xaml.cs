using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SearchAndBook.Domain;
using SearchAndBook.Repositories;
using SearchAndBook.Services;
using SearchAndBook.Shared;
using SearchAndBook.ViewModels; 
using System.Collections.Generic;
using System.Linq;

namespace SearchAndBook.Views
{
    public sealed partial class DiscoveryView : Page
    {
        public DiscoveryViewModel ViewModel { get; private set; }

        private SearchAndFilterService service;
        private List<GameDTO> allGames = new();
        private int currentPage = 1;
        private int pageSize = 10;

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

            service = new SearchAndFilterService(gamesRepository, usersRepository, rentalsRepository, geoService);

            ViewModel = new DiscoveryViewModel(service, geoService);
            this.DataContext = ViewModel;

            LoadGames();
        }

        private void LoadGames()
        {
            var criteria = new FilterCriteria();

            allGames = service.Search(criteria).ToList();

            currentPage = 1;

            DisplayPage();
        }

        private void DisplayPage()
        {
            var games = allGames
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            GamesList.ItemsSource = games;

            PageText.Text = $"Page {currentPage}";
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage * pageSize < allGames.Count)
            {
                currentPage++;
                DisplayPage();
            }
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                DisplayPage();
            }
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

        private void Game_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is GameDTO game)
            {
                Frame.Navigate(typeof(GameDetailsView), game.GameId);
            }
        }
    }
}