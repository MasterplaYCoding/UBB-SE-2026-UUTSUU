using SearchAndBook.CommandHandler;
using SearchAndBook.Services;
using SearchAndBook.Shared;
using SearchAndBook.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SearchAndBook.ViewModels
{
    internal class DiscoveryViewModel : INotifyPropertyChanged
    {
        private readonly ISearchAndFilterService _searchService;
        private const int PageSize = 10;
        public List<GameDTO> GamesAvailableTonight { get; set; } = new();
        public List<GameDTO> GamesOthers { get; set; } = new();
        public ObservableCollection<GameDTO> GamesShown { get; set; } = new();
        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged();
            }
        }
        private int _dividedState = 0;
        public int DividedState
        {
            get => _dividedState;
            set
            {
                _dividedState = value;
                OnPropertyChanged();
                CurrentPage = 1;
                RefreshPage();
            }
        }
        public FilterCriteria Filter { get; set; } = new();
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand SelectGameCommand { get; }
        public ICommand SearchCommand { get; }
        public event Action<int>? OnGameSelectedRequest;
        public event Action<FilterCriteria>? OnSearchRequest;
        public DiscoveryViewModel(ISearchAndFilterService searchService)
        {
            _searchService = searchService;
            NextPageCommand = new RelayCommand(_ => NextPage());
            PreviousPageCommand = new RelayCommand(_ => PreviousPage());
            SelectGameCommand = new RelayCommand(obj =>
            {
                if (obj is GameDTO game)
                {
                    SelectGame(game.GameId);
                }
            }
            );
            SearchCommand = new RelayCommand(_ => Search(Filter));
            LoadDiscoveryFeed();
        }
        public void LoadDiscoveryFeed()
        {
           // int userId = SessionContext.Instance.UserId;
            int userId = 1; // Placeholder for user ID, replace with actual session context retrieval
            GamesAvailableTonight = _searchService.GetFeedAvailableTonight(userId).ToList();
            GamesOthers = _searchService.GetFeedOthers(userId).ToList();
            DividedState = 0;
            CurrentPage = 1;
            RefreshPage();
        }
        public void SelectGame(int gameId)
        {
            OnGameSelectedRequest?.Invoke(gameId);
        }
        public void Search(FilterCriteria criteria)
        {
            OnSearchRequest?.Invoke(criteria);
        }
        public void NextPage()
        {
            if ((CurrentPage * PageSize) < (DividedState == 0 ? GamesAvailableTonight.Count : GamesOthers.Count))
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
        private List<GameDTO> GetCurrentSource()
        {
            return DividedState == 0 ? GamesAvailableTonight : GamesOthers;
        }
        private void RefreshPage()
        {
            GamesShown.Clear();
            var sourceGames = GetCurrentSource();
            var pageListings = sourceGames.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
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
