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
using SearchAndBook.Shared;
using Windows.UI.Notifications;
using SearchAndBook.Repositories;


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
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var criteria = e.Parameter as FilterCriteria ?? new FilterCriteria();
            var gameRepository = new GamesRepository(); 
            var userRepository = new UsersRepository();
            var service = new SearchAndFilterService(gameRepository, userRepository);
            var viewModel = new FilteredSearchViewModel(service);
            viewModel.OnGameSelectedRequest += gameId =>
            {
                Frame.Navigate(typeof(GameDetailsView), gameId);
            };
            viewModel.Initialize(criteria);
            this.DataContext = viewModel;

        }
    }
}
