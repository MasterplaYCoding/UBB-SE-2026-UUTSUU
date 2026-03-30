using Microsoft.UI.Xaml.Controls;
using SearchAndBook.Services;
using SearchAndBook.ViewModels;
using SearchAndBook.Repositories;

namespace SearchAndBook.Views
{   
    public sealed partial class GameDetailsView : Page
    {
        public GameDetailsView()
        {
            InitializeComponent();
            var gameRepo = new GamesRepository();
            var rentalRepo = new RentalsRepository();
            var userRepo = new UsersRepository();
            var service = new BookingService(gameRepo, rentalRepo, userRepo);
            this.DataContext = new GameDetailsViewModel(service, 1);
        }
    }
}
