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
        public GameDetailsView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is not int gameId) { return; }

            var gameRepo = new GamesRepository();
            var rentalRepo = new RentalsRepository();
            var userRepo = new UsersRepository();
            var service = new BookingService(gameRepo, rentalRepo, userRepo);
            var vm = new GameDetailsViewModel(service, gameId);

            vm.OnGoBackRequested += () =>
            {
                if (Frame.CanGoBack)
                    Frame.GoBack();
            };

            vm.OnStartBookingRequested += (bookingDto, range) =>
            {
                Frame.Navigate(typeof(ConfirmBookingView), (bookingDto, range));
            };

            vm.OnMessageRequested += async message =>
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

            this.DataContext = vm;
        }

        private void OnBackClicked(object sender, RoutedEventArgs e)
        {
            var vm = (GameDetailsViewModel)this.DataContext;
            vm.GoBack();
        }

        private async void OnBookClicked(object sender, RoutedEventArgs e)
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

            var vm = (GameDetailsViewModel)this.DataContext;
            var sorted = selectedDates
               .Select(d => d.DateTime)
               .OrderBy(d => d)
               .ToList();
            var range = new TimeRange(sorted[0], sorted[selectedDates.Count - 1]);
            vm.StartBooking(range);
        }

        private DateTime? _selectedStart;
        private DateTime? _selectedEnd;

        private void OnDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs e)
        {
            if (this.DataContext is not GameDetailsViewModel vm)
                return;

            var selectedDates = RentalCalendar.SelectedDates;

            if (selectedDates.Count > 2)
            {
                var toKeep = new List<DateTimeOffset>
        {
            selectedDates[selectedDates.Count - 2],
            selectedDates[selectedDates.Count - 1]
        };
                RentalCalendar.SelectedDates.Clear();
                foreach (var date in toKeep)
                    RentalCalendar.SelectedDates.Add(date);
                return;
            }

            if (selectedDates.Count < 1)
            {
                _selectedStart = null;
                _selectedEnd = null;
                ForceRedraw();
                return;
            }

            var sorted = selectedDates
                .Select(d => d.DateTime)
                .OrderBy(d => d)
                .ToList();

            _selectedStart = sorted[0];
            _selectedEnd = sorted[sorted.Count - 1];

            var range = new TimeRange(_selectedStart.Value, _selectedEnd.Value);
            vm.CalculatePrice(range);

            ForceRedraw();
        }

        private void ForceRedraw()
        {
            var current = RentalCalendar.MinDate;
            RentalCalendar.MinDate = current.AddDays(1);
            RentalCalendar.MinDate = current;
        }

        private void OnDayItemChanging(CalendarView sender, CalendarViewDayItemChangingEventArgs e)
        {
            if (this.DataContext is not GameDetailsViewModel vm)
                return;

            var date = e.Item.Date.Date;
            var today = DateTimeOffset.Now.Date;

            if (date < today)
            {
                e.Item.IsBlackout = true;
                return;
            }

            bool isUnavailable = false;
            if (vm.UnavailableTimeRanges != null)
            {
                foreach (var range in vm.UnavailableTimeRanges)
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
                e.Item.IsBlackout = true;
                e.Item.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.DarkRed);
                return;
            }

            if (_selectedStart.HasValue && _selectedEnd.HasValue &&
                date >= _selectedStart.Value.Date && date <= _selectedEnd.Value.Date)
            {
                e.Item.IsBlackout = false;
                e.Item.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Goldenrod);
                return;
            }

            e.Item.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.DarkGreen);
        }

    }
}