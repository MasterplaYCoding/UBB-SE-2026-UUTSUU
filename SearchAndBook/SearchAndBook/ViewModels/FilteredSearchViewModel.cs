using Microsoft.UI.Xaml.Media.Imaging;
using SearchAndBook.CommandHandler;
using SearchAndBook.Domain;
using SearchAndBook.Services;
using SearchAndBook.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage.Streams;

namespace SearchAndBook.ViewModels
{
    public class FilteredSearchViewModel : INotifyPropertyChanged
    {
        private readonly ISearchAndFilterService _searchService;

        public FilterCriteria CurrentFilter { get; set; } 
        public GameDTO[] BaseResults { get; private set; }
        public GameDTO[] DisplayedResults { get; private set; }
        public bool HasNoResults { get; private set; }

        public List<GameDTO> Games { get; set; } = new();

        private GameDTO? _selectedGame;
        public GameDTO? SelectedGame
        {
            get => _selectedGame;
            set
            {
                if (_selectedGame != value)
                {
                    _selectedGame = value;
                    OnPropertyChanged(nameof(SelectedGame));
                    if (_selectedGame != null)
                    {
                        SelectGame(_selectedGame.GameId);
                        _selectedGame = null;
                        OnPropertyChanged(nameof(SelectedGame));
                    }
                }
            }
        }

        public ObservableCollection<GameDTO> GamesShown { get; set; } = new();
        private readonly Dictionary<int, BitmapImage?> _gameImages = new();

        public Dictionary<int, BitmapImage?> GameImages => _gameImages;

        private BitmapImage? _selectedGameImage;
        public BitmapImage? SelectedGameImage
        {
            get => _selectedGameImage;
            set
            {
                _selectedGameImage = value;
                OnPropertyChanged();
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

        private const int PageSize = 10;

        private decimal _selectedMaximumPrice;
        public decimal SelectedMaximumPrice { 
            get => _selectedMaximumPrice; 
            set
            {
                _selectedMaximumPrice = value;
                OnPropertyChanged(nameof(SelectedMaximumPrice));
            }
        }
        private int _selectedMinimumPlayers;
        public int SelectedMinimumPlayers
        {
            get => _selectedMinimumPlayers;
            set
            {
                _selectedMinimumPlayers = value;
                OnPropertyChanged((nameof(SelectedMinimumPlayers)));
            }
        }
        private DateTimeOffset? _selectedStartDate;
        public DateTimeOffset? SelectedStartDate
        {
            get => _selectedStartDate;
            set
            {
                _selectedStartDate = value;
                OnPropertyChanged(nameof(SelectedStartDate));
            }
        }

        private DateTimeOffset? _selectedEndDate;
        public DateTimeOffset? SelectedEndDate
        {
            get => _selectedEndDate;
            set
            {
                _selectedEndDate = value;
                OnPropertyChanged(nameof(SelectedEndDate));
            }
        }
        private void UpdateAvailabilityRange()
        {
            if (SelectedStartDate.HasValue && SelectedEndDate.HasValue && SelectedStartDate.Value <= SelectedEndDate.Value)
            {
                CurrentFilter.AvailabilityRange = new TimeRange(
                    SelectedStartDate.Value.Date,
                    SelectedEndDate.Value.Date
                );
            }
            else
            {
                CurrentFilter.AvailabilityRange = null;
            }
        }
        private string? _selectedSortOption;
        public string? SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                if (_selectedSortOption != value)
                {
                    _selectedSortOption = value;
                    OnPropertyChanged(nameof(SelectedSortOption));
                }
            }
        }
        public ICommand SearchCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand SelectGameCommand { get; }
        public ICommand ApplySelectedUiFiltersCommand { get; }
        public ICommand ClearFiltersCommand { get; }

        public event Action<int>? OnGameSelectedRequest;
        public event PropertyChangedEventHandler? PropertyChanged;

        public void Initialize(FilterCriteria initialFilter)
        {
            CurrentFilter = initialFilter;
            Search(CurrentFilter);
        }

        public FilteredSearchViewModel(ISearchAndFilterService searchService)
        {

            _searchService = searchService;

            CurrentFilter = new FilterCriteria();
            BaseResults = Array.Empty<GameDTO>();
            DisplayedResults = Array.Empty<GameDTO>();
            HasNoResults = false;

            SelectedMaximumPrice = 0;
            SelectedStartDate = null;
            SelectedEndDate = null;

            SearchCommand = new RelayCommand(_ => Search(CurrentFilter));
            NextPageCommand = new RelayCommand(_ => NextPage());
            PreviousPageCommand = new RelayCommand(_ => PreviousPage());
            SelectGameCommand = new RelayCommand(obj =>
            {
                if (obj is GameDTO game)
                {
                    if (GameImages.TryGetValue(game.GameId, out var image))
                    {
                        SelectedGameImage = image;
                    }
                    else
                    {
                        SelectedGameImage = null;
                    }

                    SelectGame(game.GameId);
                }
            });
            ApplySelectedUiFiltersCommand = new RelayCommand(_ => ApplySelectedUiFilters());
            ClearFiltersCommand = new RelayCommand(_ => ClearAllFilters());
        }

        public void LoadSearchResults(FilterCriteria searchCriteria)
        {
            BaseResults = _searchService.Search(searchCriteria);
            DisplayedResults = BaseResults;
            Games = DisplayedResults.ToList();
            CurrentPage = 1;
            RefreshPage();
            UpdateNoResultsState();
        }

        public void LoadDiscoveryResutls(GameDTO[] discoveryResults)
        {
            BaseResults = discoveryResults;
            DisplayedResults = BaseResults;
            Games = DisplayedResults.ToList();
            CurrentPage = 1;
            RefreshPage();
            UpdateNoResultsState();
        }

        public void ApplyFilters()
        {
            if (!CurrentFilter.HasValidAvailabilityRange())
            {
                return;
            }

            DisplayedResults = _searchService.ApplyFilters(BaseResults, CurrentFilter);
            Games = DisplayedResults.ToList();
            CurrentPage = 1;
            RefreshPage();
            UpdateNoResultsState();
        }

        public void ApplySelectedUiFilters()
        {
           
            if (!HasValidPlayersValue() || !HasValidDateRange())
            {
                return;
            }
            UpdateAvailabilityRange();

            CurrentFilter.MaximumPrice = (decimal)SelectedMaximumPrice;
            CurrentFilter.PlayerCount = (int)SelectedMinimumPlayers;

            if (SelectedSortOption == "Price: lowest to highest")
            {
                CurrentFilter.SortOption = SortOption.PriceAscending;
            }
            else if (SelectedSortOption == "Price: highest to lowest")
            {
                CurrentFilter.SortOption = SortOption.PriceDescending;
            }
            else
            {
                CurrentFilter.SortOption = SortOption.None;
            }

            ApplyFilters();
        }


        public bool HasValidPlayersValue()
        {
            return SelectedMinimumPlayers >= 0;
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

        public void RemoveNameFilter()
        {
            CurrentFilter.Name = null;
            ApplyFilters();
        }

        public void RemoveCityFilter()
        {
            CurrentFilter.City = null;
            ApplyFilters();
        }

        public void RemovePriceFilter()
        {
            CurrentFilter.MaximumPrice = null;
            ApplyFilters();
        }

        public void RemovePlayersFilter()
        {
            CurrentFilter.PlayerCount = null;
            ApplyFilters();
        }

        public void RemoveDateFilter()
        {
            CurrentFilter.AvailabilityRange = null;
            ApplyFilters();
        }

        public void SetPriceAscendingSort()
        {
            CurrentFilter.SortOption = SortOption.PriceAscending;
            ApplyFilters();
        }

        public void SetPriceDescendingSort()
        {
            CurrentFilter.SortOption = SortOption.PriceDescending;
            ApplyFilters();
        }

        public void ClearSorting()
        {
            CurrentFilter.SortOption = SortOption.None;
            ApplyFilters();
        }

        public void ClearAllFilters()
        {
            CurrentFilter.Reset();
            SelectedMaximumPrice = 1;
            SelectedMinimumPlayers = 1;
            SelectedStartDate = null;
            SelectedEndDate = null;
            SelectedSortOption = null;
            DisplayedResults = BaseResults;
            Games = DisplayedResults.ToList();
            CurrentPage = 1;
            RefreshPage();
            UpdateNoResultsState();
        }

        private void UpdateNoResultsState()
        {
            HasNoResults = DisplayedResults.Length == 0;
            OnPropertyChanged(nameof(HasNoResults));
        }

        public void SelectGame(int gameId)
        {
            OnGameSelectedRequest?.Invoke(gameId);
        }

        public void Search(FilterCriteria criteria)
        {
            if (!HasValidDateRange())
            {
                return;
            }
            UpdateAvailabilityRange();
            Games = _searchService.Search(criteria).ToList();
            DisplayedResults = Games.ToArray();
            BaseResults = DisplayedResults;
            CurrentPage = 1;
            RefreshPage();
            UpdateNoResultsState();
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

            var pageListings = Games
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize);

            foreach (var game in pageListings)
            {
                GamesShown.Add(game);
                LoadGameImage(game);
            }

            OnPropertyChanged(nameof(GamesShown));
        }
        private async void LoadGameImage(GameDTO game)
        {
            if (game.Image == null || game.Image.Length == 0)
            {
                _gameImages[game.GameId] = null;
                return;
            }

            using var stream = new InMemoryRandomAccessStream();
            await stream.WriteAsync(game.Image.AsBuffer());
            stream.Seek(0);

            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(stream);

            _gameImages[game.GameId] = bitmap;
        }

        public BitmapImage? GetGameImage(int gameId)
        {
            return _gameImages.TryGetValue(gameId, out var image) ? image : null;
        }

        protected void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}