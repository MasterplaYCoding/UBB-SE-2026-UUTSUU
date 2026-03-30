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
using System.Runtime.CompilerServices;
using SearchAndBook.CommandHandler;

namespace SearchAndBook.ViewModels
{
    public class FilteredSearchViewModel : INotifyPropertyChanged
    {
        private readonly ISearchAndFilterService _searchService;

        public FilterCriteria CurrentFilter { get; set; }
        public FilterCriteria Filter { get; set; } = new();

        public GameDTO[] BaseResults { get; private set; }
        public GameDTO[] DisplayedResults { get; private set; }
        public bool HasNoResults { get; private set; }

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

        private const int PageSize = 10;

        public decimal? SelectedMaximumPrice { get; set; }
        public int? SelectedMinimumPlayers { get; set; }

        public DateTime? SelectedStartDate { get; set; }
        public DateTime? SelectedEndDate { get; set; }

        public ICommand SearchCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand SelectGameCommand { get; }

        public event Action<int>? OnGameSelectedRequest;
        public event PropertyChangedEventHandler? PropertyChanged;

        public void Initialize(FilterCriteria initialFilter)
        {
            Filter = initialFilter;
            Search(Filter);
        }

        public FilteredSearchViewModel(ISearchAndFilterService searchService)
        {

            _searchService = searchService;

            CurrentFilter = new FilterCriteria();
            BaseResults = Array.Empty<GameDTO>();
            DisplayedResults = Array.Empty<GameDTO>();
            HasNoResults = false;

            SelectedMaximumPrice = null;
            SelectedStartDate = null;
            SelectedEndDate = null;

            SearchCommand = new RelayCommand(_ => Search(Filter));
            NextPageCommand = new RelayCommand(_ => NextPage());
            PreviousPageCommand = new RelayCommand(_ => PreviousPage());
            SelectGameCommand = new RelayCommand(obj =>
            {
                if (obj is GameDTO game)
                {
                    SelectGame(game.GameId);
                }
            });
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
            if (!HasValidPlayersValue())
            {
                return;
            }

            CurrentFilter.MaximumPrice = SelectedMaximumPrice;
            CurrentFilter.PlayerCount = SelectedMinimumPlayers;

            ApplyFilters();
        }


        public bool HasValidPlayersValue()
        {
            if (!SelectedMinimumPlayers.HasValue)
            {
                return true;
            }

            return SelectedMinimumPlayers.Value > 0;
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
            var pageListings = Games.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
            foreach (var game in pageListings)
            {
                GamesShown.Add(game);
            }
            OnPropertyChanged(nameof(GamesShown));
        }

        protected void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}