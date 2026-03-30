using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

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
            var gameRepo = new GameRepository();
            var rentalRepo = new RentalRepository();
            var userRepo = new UserRepository();
            var service = new BookingService(gameRepo, rentalRepo, userRepo);
            this.DataContext = new GameDetailsViewModel(service, 1);
        }
    }
}
