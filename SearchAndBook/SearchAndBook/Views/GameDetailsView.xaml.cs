using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SearchAndBook.Domain;
using SearchAndBook.Repositories;
using SearchAndBook.Services;
using SearchAndBook.Shared;
using SearchAndBook.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SearchAndBook.Views
{
    public sealed partial class GameDetailsView : Page
    {

        private DateTime? _selectedDateStart;
        private DateTime? _selectedDateEnd;

        public GameDetailsView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs eventArgs)
        {
            base.OnNavigatedTo(eventArgs);
            if (eventArgs.Parameter is not int gameId) { return; }

            var gameRepository = new GamesRepository();
            var rentalRepository = new RentalsRepository();
            var userRepository = new UsersRepository();
            var service = new BookingService(gameRepository, rentalRepository, userRepository);
            var viewModel = new GameDetailsViewModel(service, gameId);

            viewModel.OnGoBackRequested += () =>
            {
                if (Frame.CanGoBack)
                    Frame.GoBack();
            };

            viewModel.OnStartBookingRequested += (bookingDto, range) =>
            {
                Frame.Navigate(typeof(ConfirmBookingView), (bookingDto, range));
            };

            viewModel.OnMessageRequested += async message =>
            {
                var dialog = new ContentDialog
                {
                    Title = "Booking",
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await dialog.ShowAsync();
            };

            this.DataContext = viewModel;
        }

        private void OnBackClicked(object sender, RoutedEventArgs eventArgs)
        {
            var viewModel = (GameDetailsViewModel) this.DataContext;
            viewModel.GoBack();
        }

        private async void OnBookClicked(object sender, RoutedEventArgs eventArgs)
        {
            var selectedDates = RentalCalendar.SelectedDates;
            if (selectedDates.Count == 0)
            {
                var dialog = new ContentDialog
                {
                    Title = "Invalid selection",
                    Content = "Please select at least one date.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            var viewModel = (GameDetailsViewModel)this.DataContext;
            var sortedDates = selectedDates
               .Select(date => date.DateTime)
               .OrderBy(date => date)
               .ToList();
            var dateRange = new TimeRange(sortedDates[0], sortedDates[selectedDates.Count - 1]);
            viewModel.StartBooking(dateRange);
        }

        private void OnDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs eventArgs)
        {
            if (this.DataContext is not GameDetailsViewModel viewModel)
                return;

            var selectedDates = RentalCalendar.SelectedDates;

            if (selectedDates.Count > 2)
            {
                var datesToKeep = new List<DateTimeOffset>
                    {
                        selectedDates[selectedDates.Count - 2],
                        selectedDates[selectedDates.Count - 1]
                    };
                RentalCalendar.SelectedDates.Clear();
                foreach (var date in datesToKeep)
                    RentalCalendar.SelectedDates.Add(date);
                return;
            }

            if (selectedDates.Count < 1)
            {
                _selectedDateStart = null;
                _selectedDateEnd = null;
                ForceRedrawCalendar();
                return;
            }

            var sorted = selectedDates
                .Select(d => d.DateTime)
                .OrderBy(d => d)
                .ToList();

            _selectedDateStart = sorted[0];
            _selectedDateEnd = sorted[sorted.Count - 1];

            var range = new TimeRange(_selectedDateStart.Value, _selectedDateEnd.Value);
            viewModel.CalculatePrice(range);

            ForceRedrawCalendar();
        }

        private void ForceRedrawCalendar()
        {
            var currentDate = RentalCalendar.MinDate;
            RentalCalendar.MinDate = currentDate.AddDays(1);
            RentalCalendar.MinDate = currentDate;
        }

        private void OnDayItemChanging(CalendarView sender, CalendarViewDayItemChangingEventArgs eventArgs)
        {
            if (this.DataContext is not GameDetailsViewModel viewModel)
                return;

            var date = eventArgs.Item.Date.Date;
            var today = DateTimeOffset.Now.Date;

            if (date < today)
            {
                eventArgs.Item.IsBlackout = true;
                return;
            }

            bool isUnavailable = false;
            if (viewModel.UnavailableTimeRanges != null)
            {
                foreach (var range in viewModel.UnavailableTimeRanges)
                {
                    if (date >= range.StartTime.Date && date <= range.EndTime.Date)
                    {
                        isUnavailable = true;
                        break;
                    }
                }
            }

            if (isUnavailable)
            {
                eventArgs.Item.IsBlackout = true;
                eventArgs.Item.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.DarkRed);
                return;
            }

            if (_selectedDateStart.HasValue && _selectedDateEnd.HasValue &&
                date >= _selectedDateStart.Value.Date && date <= _selectedDateEnd.Value.Date)
            {
                eventArgs.Item.IsBlackout = false;
                eventArgs.Item.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Goldenrod);
                return;
            }

            eventArgs.Item.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.DarkGreen);
        }
    }
}