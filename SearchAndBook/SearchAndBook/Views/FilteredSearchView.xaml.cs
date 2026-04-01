using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SearchAndBook.Repositories;
using SearchAndBook.Services;
using SearchAndBook.Shared;
using SearchAndBook.ViewModels;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SearchAndBook.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FilteredSearchView : Page
    {
        public FilteredSearchView()
        {
            InitializeComponent();
        }
        public FilteredSearchViewModel ViewModel { get; private set; }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var criteria = e.Parameter as FilterCriteria ?? new FilterCriteria();
            var gamesRepository = new GamesRepository();
            var usersRepository = new UsersRepository();
            var rentalsRepository = new RentalsRepository();
            var geoService = App.GlobalGeoService!;
            var service = new SearchAndFilterService(gamesRepository, usersRepository, rentalsRepository, geoService);
            ViewModel = new FilteredSearchViewModel(service,geoService);
            ViewModel.OnGameSelectedRequest += gameId =>
            {
                Frame.Navigate(typeof(GameDetailsView), gameId);
            };
            ViewModel.OnGoBackRequest += () => Frame.Navigate(typeof(DiscoveryView));
            this.DataContext = ViewModel;
            ViewModel.Initialize(criteria);
        }
    }
}
