using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using SearchAndBook.CommandHandler;
using SearchAndBook.Domain;
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

        public event Action? OnPageChanged;

        private bool _showOthersHeader;
        public bool ShowOthersHeader
        {
            get => _showOthersHeader;
            set
            {
                _showOthersHeader = value;
                OnPropertyChanged(nameof(ShowOthersHeader));
            }
        }

        public FilterCriteria Filter { get; set; } = new();

        public DateTimeOffset MinEndDate => SelectedStartDate.HasValue
            ? SelectedStartDate.Value.AddDays(1) : Today;

        private DateTimeOffset? _selectedStartDate;
        public DateTimeOffset? SelectedStartDate
        {
            get => _selectedStartDate;
            set
            {
                _selectedStartDate = value;
                OnPropertyChanged(nameof(SelectedStartDate));
                OnPropertyChanged(nameof(MinEndDate));

                _selectedEndDate = null;
                OnPropertyChanged(nameof(SelectedEndDate));
            }
        }

        public DateTimeOffset MinStartDate => Today;

        private DateTimeOffset? _selectedEndDate;
        public DateTimeOffset? SelectedEndDate
        {
            get => _selectedEndDate;
            set
            {
                if (value.HasValue && SelectedStartDate.HasValue && value.Value <= SelectedStartDate.Value)
                {
                    _selectedEndDate = null;
                    OnPropertyChanged(nameof(SelectedEndDate));
                    return;
                }
                _selectedEndDate = value;
                OnPropertyChanged(nameof(SelectedEndDate));
            }
        }

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
                return (int)Math.Ceiling((double)TotalGamesCount / PageSize);
            }
        }

        public bool HasPagedAvailableTonight => PagedGamesAvailableTonight.Any();
        public bool HasPagedOthers => PagedGamesOthers.Any();

        public string OthersTitle => HasPagedOthers ? "Others" : " ";

        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand SearchCommand { get; }

        public event Action<int>? OnGameSelectedRequest;
        public event Action<FilterCriteria>? OnSearchRequest;
        public event PropertyChangedEventHandler? PropertyChanged;

        public string StartDatePlaceholder => "Today";
        public string EndDatePlaceholder => "Tomorrow";

        public DateTimeOffset Today => DateTimeOffset.Now.Date;
        public DateTimeOffset Tomorrow => DateTimeOffset.Now.Date.AddDays(1);

        public DiscoveryViewModel(ISearchAndFilterService searchService, IGeoService geoService)
        {
            _searchService = searchService;
            _geoService = geoService;

            _selectedStartDate = null;
            _selectedEndDate = null;

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
                OnPageChanged?.Invoke();
            }
        }

        public void PreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                RefreshPage();
                OnPageChanged?.Invoke();
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
            OnPropertyChanged(nameof(OthersTitle));
        }

        public void Search(FilterCriteria criteria)
        {
            if (!HasValidDateRange())
            {
                return;
            }

            Filter.Name = criteria.Name;
            Filter.City = criteria.City;
            Filter.SortOption = criteria.SortOption;
            Filter.MaximumPrice = criteria.MaximumPrice;
            Filter.PlayerCount = criteria.PlayerCount;
            Filter.UserId = SessionContext.GetInstance().UserId;

            UpdateAvailabilityRange();

            CurrentPage = 1;
            OnSearchRequest?.Invoke(Filter);
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

        private void UpdateAvailabilityRange()
        {
            if (SelectedStartDate.HasValue &&
                SelectedEndDate.HasValue &&
                SelectedStartDate.Value <= SelectedEndDate.Value)
            {
                Filter.AvailabilityRange = new TimeRange(
                    SelectedStartDate.Value.Date,
                    SelectedEndDate.Value.Date
                );
            }
            else
            {
                Filter.AvailabilityRange = null;
            }
        }

        public bool HasValidDateRange()
        {
            if (!SelectedStartDate.HasValue && !SelectedEndDate.HasValue)
            {
                return true;
            }

            if (!SelectedStartDate.HasValue || !SelectedEndDate.HasValue)
            {
                return false;
            }

            return SelectedStartDate.Value < SelectedEndDate.Value;
        }
    }
}