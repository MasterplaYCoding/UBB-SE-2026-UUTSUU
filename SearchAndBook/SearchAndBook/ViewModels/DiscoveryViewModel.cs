using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using SearchAndBook.CommandHandler;
using SearchAndBook.Services;
using SearchAndBook.Shared;
using SearchAndBook.Utils;

namespace SearchAndBook.ViewModels
{
    public class DiscoveryViewModel : INotifyPropertyChanged
    {
        private readonly ISearchAndFilterService _searchService;
        private readonly IGeoService _geoService;

        private const int PageSize = 10;

        public List<GameDTO> GamesAvailableTonight { get; set; } = new();
        public List<GameDTO> GamesOthers { get; set; } = new();

        public ObservableCollection<GameDTO> PagedGamesAvailableTonight { get; } = new();
        public ObservableCollection<GameDTO> PagedGamesOthers { get; } = new();

        public FilterCriteria Filter { get; set; } = new();

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
        private int TotalGamesCount => GamesAvailableTonight.Count + GamesOthers.Count;
        public int TotalPages
        {
            get
            {
                if (TotalGamesCount == 0)
                    return 1;
                return (int)Math.Ceiling((double)TotalGamesCount/ PageSize);
            }
        }

        public bool HasPagedAvailableTonight => PagedGamesAvailableTonight.Any();
        public bool HasPagedOthers => PagedGamesOthers.Any();

        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand SearchCommand { get; }

        public event Action<int>? OnGameSelectedRequest;
        public event Action<FilterCriteria>? OnSearchRequest;
        public event PropertyChangedEventHandler? PropertyChanged;

        public DiscoveryViewModel(ISearchAndFilterService searchService, IGeoService geoService)
        {
            _searchService = searchService;
            _geoService = geoService;

            NextPageCommand = new RelayCommand(_ => NextPage());
            PreviousPageCommand = new RelayCommand(_ => PreviousPage());
            SearchCommand = new RelayCommand(_ => Search(Filter));

            LoadDiscoveryFeed();
        }
        public string NoResultsMessage => TotalGamesCount == 0 ? "No games available." : "";

        public async void LoadDiscoveryFeed()
        {
            int userId = SessionContext.GetInstance().UserId;

            GamesAvailableTonight = _searchService.GetFeedAvailableTonight(userId).ToList();
            GamesOthers = _searchService.GetFeedOthers(userId).ToList();

            await LoadImagesForGames(GamesAvailableTonight);
            await LoadImagesForGames(GamesOthers);

            CurrentPage = 1;
            RefreshPage();
            OnPropertyChanged(nameof(TotalPages));
            OnPropertyChanged(nameof(GamesAvailableTonight));
            OnPropertyChanged(nameof(GamesOthers));
            OnPropertyChanged(nameof(NoResultsMessage));
        }

        private async Task LoadImagesForGames(IEnumerable<GameDTO> games)
        {
            foreach (var game in games)
            {
                if (game.Image != null && game.GameImage == null)
                {
                    game.GameImage = await GameImage.ToBitmapImage(game.Image);
                }
            }
        }

        public void NextPage()
        {
            if (CurrentPage * PageSize < TotalGamesCount)
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
            PagedGamesAvailableTonight.Clear();
            PagedGamesOthers.Clear();

            int globalStart = (CurrentPage - 1) * PageSize;
            int remaining = PageSize;

            // Take from AvailableTonight first
            if (globalStart < GamesAvailableTonight.Count)
            {
                var availableSlice = GamesAvailableTonight
                    .Skip(globalStart)
                    .Take(remaining)
                    .ToList();

                foreach (var game in availableSlice)
                {
                    PagedGamesAvailableTonight.Add(game);
                }

                remaining -= availableSlice.Count;
                globalStart = 0;
            }
            else
            {
                globalStart -= GamesAvailableTonight.Count;
            }

            // Then continue with Others if page still has room
            if (remaining > 0)
            {
                var othersSlice = GamesOthers
                    .Skip(globalStart)
                    .Take(remaining)
                    .ToList();

                foreach (var game in othersSlice)
                {
                    PagedGamesOthers.Add(game);
                }
            }
            OnPropertyChanged(nameof(PagedGamesAvailableTonight));
            OnPropertyChanged(nameof(PagedGamesOthers));
            OnPropertyChanged(nameof(HasPagedAvailableTonight));
            OnPropertyChanged(nameof(HasPagedOthers));
        }

        public void Search(FilterCriteria criteria)
        {
            OnSearchRequest?.Invoke(criteria);
        }

        public ObservableCollection<string> CitySuggestions { get; } = new();

        private string _citySearchText = string.Empty;
        public string CitySearchText
        {
            get => _citySearchText;
            set
            {
                if (_citySearchText != value)
                {
                    _citySearchText = value;
                    OnPropertyChanged(nameof(CitySearchText));

                    Filter.City = value;
                    UpdateCitySuggestions(value);
                }
            }
        }

        private void UpdateCitySuggestions(string input)
        {
            CitySuggestions.Clear();

            if (!string.IsNullOrWhiteSpace(input) && input.Length >= 2)
            {
                var matches = _geoService.GetCitySuggestions(input);
                foreach (var match in matches)
                {
                    CitySuggestions.Add(match);
                }
            }
        }

        protected void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}