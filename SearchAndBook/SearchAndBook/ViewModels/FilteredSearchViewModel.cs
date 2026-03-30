using SearchAndBook.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SearchAndBook.Shared;

namespace SearchAndBook.ViewModels
{
    internal class FilteredSearchViewModel
    {
        public ISearchAndFilterService SearchService { get; set; }
        public FilterCriteria CurrentFilter { get; set; }
        public GameDTO[] BaseResults { get; private set; }
        public GameDTO[] DisplayedResults { get; private set; }
        public bool HasNoResults { get; private set; }

        public decimal? SelectedMinimumPrice { get; set; }
        public decimal? SelectedMaximumPrice { get; set; }
        public int? SelectedMinimumPlayers { get; set; }

        public DateTime? SelectedStartDate { get; set; }
        public DateTime? SelectedEndDate { get; set; }
        public FilteredSearchViewModel(ISearchAndFilterService searchService) {
            SearchService = searchService;
            CurrentFilter = new FilterCriteria();
            BaseResults = Array.Empty<GameDTO>();
            DisplayedResults = Array.Empty<GameDTO>();
            HasNoResults = false;

            SelectedMaximumPrice = null;
            SelectedMinimumPrice = null;
            SelectedMinimumPlayers = null;
            SelectedStartDate = null;
            SelectedEndDate = null;
        }

        public void LoadSearchResults(FilterCriteria searchCriteria) {
            BaseResults = SearchService.Search(searchCriteria);
            DisplayedResults = BaseResults;
            UpdateNoResultsState();

        }

        public void LoadDiscoveryResutls(GameDTO[] discoveryResults) {
            BaseResults = discoveryResults;
            DisplayedResults = BaseResults;
            UpdateNoResultsState();

        }

        public void ApplyFilters() {

            if (!CurrentFilter.HasValidAvailabilityRange()) {
                return;
            }

            DisplayedResults = SearchService.ApplyFilters(BaseResults, CurrentFilter);
            UpdateNoResultsState();
        }

        public void ApplySelectedUiFilters()
        {
            if (!HasValidPriceRange() || !HasValidPlayersValue())
            {
                return;
            }

            CurrentFilter.MinimumPrice = SelectedMinimumPrice;
            CurrentFilter.MaximumPrice = SelectedMaximumPrice;
            CurrentFilter.MinimumPlayers = SelectedMinimumPlayers;

            ApplyFilters();
        }

        public bool HasValidPriceRange()
        {
            if (!SelectedMinimumPrice.HasValue || !SelectedMaximumPrice.HasValue)
            {
                return true;
            }

            return SelectedMinimumPrice.Value <= SelectedMaximumPrice.Value;
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



        public void RemoveNameFilter() { 
            CurrentFilter.Name = null;
            ApplyFilters();
        }

        public void RemoveCityFilter() {
            CurrentFilter.City = null;
            ApplyFilters();
        }

        public void RemovePriceFilter() {
            CurrentFilter.MinimumPrice = null;
            CurrentFilter.MaximumPrice = null;
            ApplyFilters();
        }

        public void RemovePlayersFilter() {
            CurrentFilter.MinimumPlayers = null;
            ApplyFilters();
        }

        public void RemoveDateFilter() { 
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
            UpdateNoResultsState();
        }
        private void UpdateNoResultsState()
        {
            HasNoResults = DisplayedResults.Length == 0;
        }


    }
}
