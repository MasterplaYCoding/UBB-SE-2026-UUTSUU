using SearchAndBook.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SearchAndBook.Shared;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SearchAndBook.CommandHandler;

namespace SearchAndBook.ViewModels
{
    internal class FilteredSearchViewModel : INotifyPropertyChanged
    {
        private readonly ISearchAndFilterService _searchService;
        public List<GameDTO> Games { get; set; } = new();
        public ObservableCollection<GameDTO> GamesShown { get; set; } = new();
        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }
        public FilterCriteria Filter { get; set; } = new();
        private const int PageSize = 10;
        public ICommand SearchCommand { get; }
        public ICommand NextPageCommand { get;}
        public ICommand PreviousPageCommand { get; }
        public void Initialize(FilterCriteria initialFilter)
        {
            Filter = initialFilter;
            Search(Filter);
        }
        public FilteredSearchViewModel(ISearchAndFilterService searchService)
        {
            _searchService = searchService;
            SearchCommand = new RelayCommand(_ => Search(Filter));
            NextPageCommand = new RelayCommand(_ => NextPage());
            PreviousPageCommand = new RelayCommand(_ => PreviousPage());
        }

        public void SelectGame(int gameId)
        {
            //null
        }
        public void Search(FilterCriteria criteria)
        {
            Games = _searchService.search(criteria);
            CurrentPage = 1;
            RefreshPage();
        }
        public void NextPage()
        {
            if (CurrentPage * PageSize < Games.Count)
            {
                CurrentPage++;
                RefreshPage();
            }
        }
        public void PreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                RefreshPage();
             
            }
        }
        private void RefreshPage()
        {
            GamesShown.Clear();
            var pageListings = Games.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
            foreach (var game in pageListings)
            {
                GamesShown.Add(game);
            }
            OnPropertyChanged(nameof(GamesShown));
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
