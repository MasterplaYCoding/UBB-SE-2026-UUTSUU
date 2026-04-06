using SearchAndBook.CommandHandler;
using SearchAndBook.Domain;
using SearchAndBook.Services;
using SearchAndBook.Shared;
using SearchAndBook.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SearchAndBook.ViewModels
{
    public class DiscoveryViewModel : INotifyPropertyChanged
    {
        private readonly InterfaceSearchAndFilterService _searchService;
        private readonly InterfaceGeographicalService _geographicalService;

        private const int PageSize = 10;

        public List<GameDTO> GamesAvailableTonight { get; set; } = new();
        public List<GameDTO> GamesOthers { get; set; } = new();

        public ObservableCollection<GameDTO> PagedGamesAvailableTonight { get; } = new();
        public ObservableCollection<GameDTO> PagedGamesOthers { get; } = new();

        public bool IsEndDateEnabled => SelectedStartDate.HasValue;

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

        public DateTimeOffset MinStartDate => DateTimeOffset.Now.Date;

        public DateTimeOffset MinEndDate =>
        SelectedStartDate.HasValue
        ? new DateTimeOffset(SelectedStartDate.Value.Year, SelectedStartDate.Value.Month, 1, 0, 0, 0, TimeSpan.Zero)
        : DateTimeOffset.Now.Date;

        private DateTimeOffset? _selectedStartDate;
        public DateTimeOffset? SelectedStartDate
        {
            get => _selectedStartDate;
            set
            {
                var newValue = value?.Date;

                if (_selectedStartDate != newValue)
                {
                    _selectedStartDate = newValue;
                    OnPropertyChanged(nameof(SelectedStartDate));
                    OnPropertyChanged(nameof(MinEndDate));
                    OnPropertyChanged(nameof(IsEndDateEnabled));

                    if (_selectedStartDate.HasValue)
                    {
                        _selectedEndDate = _selectedStartDate.Value;
                    }
                    else
                    {
                        _selectedEndDate = null;
                    }

                    OnPropertyChanged(nameof(SelectedEndDate));
                }
            }
        }

        private DateTimeOffset? _selectedEndDate;
        public DateTimeOffset? SelectedEndDate
        {
            get => _selectedEndDate;
            set
            {
                var newValue = value?.Date;

                if (SelectedStartDate.HasValue && newValue.HasValue &&
                    newValue.Value < SelectedStartDate.Value)
                {
                    newValue = SelectedStartDate.Value.Date;
                }

                _selectedEndDate = newValue;
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
        public event Action<string>? OnErrorOccurred;
        public event PropertyChangedEventHandler? PropertyChanged;

        public DiscoveryViewModel(InterfaceSearchAndFilterService searchService, InterfaceGeographicalService geographicalService)
        {
            _searchService = searchService;
            _geographicalService = geographicalService;

            _selectedStartDate = null;
            _selectedEndDate = null;

            NextPageCommand = new RelayCommand(_ => NextPage());
            PreviousPageCommand = new RelayCommand(_ => PreviousPage());
            SearchCommand = new RelayCommand(_ => SearchGamesByFilter(Filter));

            try
            {
                LoadDiscoveryFeed();
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"Could not load discovery feed. {ex.Message}");
            }
        }

        public string NoResultsMessage => TotalGamesCount == 0 ? "No games available." : "";

        public async void LoadDiscoveryFeed()
        {
            try
            {
                int userId = SessionContext.GetInstance().UserId;

                GamesAvailableTonight = _searchService.GetGamesFeedAvailableTonightByUser(userId).ToList();
                GamesOthers = _searchService.GetOtherGamesFeedByUser(userId).ToList();

                await LoadImagesForGames(GamesAvailableTonight);
                await LoadImagesForGames(GamesOthers);

                CurrentPage = 1;
                RefreshPage();
                OnPropertyChanged(nameof(TotalPages));
                OnPropertyChanged(nameof(GamesAvailableTonight));
                OnPropertyChanged(nameof(GamesOthers));
                OnPropertyChanged(nameof(NoResultsMessage));
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"Could not load discovery feed. {ex.Message}");
            }
        }

        private async Task LoadImagesForGames(IEnumerable<GameDTO> games)
        {
            try
            {
                foreach (var game in games)
                {
                    if (game.Image != null && game.GameImage == null)
                    {
                        try
                        {
                            game.GameImage = await GameImage.ToBitmapImage(game.Image);
                        }
                        catch (Exception ex)
                        {
                            OnErrorOccurred?.Invoke($"Could not load an image for a game. {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"Could not load game images. {ex.Message}");
            }
        }

        public void NextPage()
        {
            try
            {
                if (CurrentPage * PageSize < TotalGamesCount)
                {
                    CurrentPage++;
                    RefreshPage();
                    OnPageChanged?.Invoke();
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"Could not go to next page. {ex.Message}");
            }
        }

        public void PreviousPage()
        {
            try
            {
                if (CurrentPage > 1)
                {
                    CurrentPage--;
                    RefreshPage();
                    OnPageChanged?.Invoke();
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"Could not go to previous page. {ex.Message}");
            }
        }

        private void RefreshPage()
        {
            try
            {
                PagedGamesAvailableTonight.Clear();
                PagedGamesOthers.Clear();

                int globalStart = (CurrentPage - 1) * PageSize;
                int remaining = PageSize;

                if (globalStart < GamesAvailableTonight.Count)
                {
                    var availableSlice = GamesAvailableTonight
                        .Skip(globalStart)
                        .Take(remaining)
                        .ToList();

                    foreach (var game in availableSlice)
                        PagedGamesAvailableTonight.Add(game);

                    remaining -= availableSlice.Count;
                    globalStart = 0;
                }
                else
                {
                    globalStart -= GamesAvailableTonight.Count;
                }

                if (remaining > 0)
                {
                    var othersSlice = GamesOthers
                        .Skip(globalStart)
                        .Take(remaining)
                        .ToList();

                    foreach (var game in othersSlice)
                        PagedGamesOthers.Add(game);
                }

                OnPropertyChanged(nameof(PagedGamesAvailableTonight));
                OnPropertyChanged(nameof(PagedGamesOthers));
                OnPropertyChanged(nameof(HasPagedAvailableTonight));
                OnPropertyChanged(nameof(HasPagedOthers));
                OnPropertyChanged(nameof(OthersTitle));
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"Could not refresh page results. {ex.Message}");
            }
        }

        public void SearchGamesByFilter(FilterCriteria criteria)
        {
            try
            {
                if (!HasValidDateRange())
                    return;

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
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"Could not perform search. {ex.Message}");
            }
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
            try
            {
                CitySuggestions.Clear();

                if (!string.IsNullOrWhiteSpace(input) && input.Length >= 2)
                {
                    var matches = _geographicalService.GetCitySuggestions(input);
                    foreach (var match in matches)
                        CitySuggestions.Add(match);
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"Could not load city suggestions. {ex.Message}");
            }
        }

        protected void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateAvailabilityRange()
        {
            try
            {
                if (SelectedStartDate.HasValue &&
                    SelectedEndDate.HasValue &&
                    SelectedStartDate.Value <= SelectedEndDate.Value)
                {
                    Filter.AvailabilityRange = new TimeRange(
                        SelectedStartDate.Value.Date,
                        SelectedEndDate.Value.Date);
                }
                else
                {
                    Filter.AvailabilityRange = null;
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"Could not update availability range. {ex.Message}");
            }
        }

        public bool HasValidDateRange()
        {
            try
            {
                if (!SelectedStartDate.HasValue && !SelectedEndDate.HasValue)
                    return true;

                if (!SelectedStartDate.HasValue || !SelectedEndDate.HasValue)
                    return false;

                return SelectedStartDate.Value <= SelectedEndDate.Value;
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"Could not validate date range. {ex.Message}");
                return false;
            }
        }
    }
}