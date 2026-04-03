using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SearchAndBook.Domain;
using SearchAndBook.Repositories;
using SearchAndBook.Services;
using SearchAndBook.Shared;
using SearchAndBook.ViewModels;

namespace SearchAndBook.Views;

public sealed partial class ConfirmBookingView : Page
{
    public ConfirmBookingView()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is not (BookingDTO bookingDTO, TimeRange range))
            return;

        var gameRepo = new GamesRepository();
        var rentalRepo = new RentalsRepository();
        var userRepo = new UsersRepository();
        var service = new BookingService(gameRepo, rentalRepo, userRepo);
        var vm = new ConfirmBookingViewModel(service, bookingDTO, range);

        vm.OnGoBackRequested += () =>
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        };

        vm.OnConfirmBookingRequested += async () =>
        {
            var dialog = new ContentDialog
            {
                Title = "Success",
                Content = "Booking request was sent successfully!",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
            Frame.Navigate(typeof(DiscoveryView));
        };

        this.DataContext = vm;
    }

    private void OnBackClicked(object sender, RoutedEventArgs e)
    {
        var vm = (ConfirmBookingViewModel)this.DataContext;
        vm.GoBack();
    }

    private DateTime? _modifySelectedStart;
    private DateTime? _modifySelectedEnd;

    private async void OnModifyClicked(object sender, RoutedEventArgs e)
    {
        var vm = (ConfirmBookingViewModel)this.DataContext;

        var calendar = new CalendarView
        {
            SelectionMode = CalendarViewSelectionMode.Multiple,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            MinDate = DateTimeOffset.Now.Date,
            SelectedBorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Goldenrod),
            SelectedHoverBorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Goldenrod),
            SelectedPressedBorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Goldenrod),
        };

        calendar.CalendarViewDayItemChanging += (calSender, args) =>
        {
            var date = args.Item.Date.DateTime;

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
                args.Item.IsBlackout = true;
                args.Item.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.DarkRed);
                return;
            }

            if (_modifySelectedStart.HasValue && _modifySelectedEnd.HasValue &&
                date.Date >= _modifySelectedStart.Value.Date && date.Date <= _modifySelectedEnd.Value.Date)
            {
                args.Item.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Goldenrod);
                return;
            }

            args.Item.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.DarkGreen);
        };

        calendar.SelectedDatesChanged += (calSender, args) =>
        {
            var selectedDates = calSender.SelectedDates;
            if (selectedDates.Count > 2)
            {
                var toKeep = new List<DateTimeOffset>
        {
            selectedDates[selectedDates.Count - 2],
            selectedDates[selectedDates.Count - 1]
        };
                calSender.SelectedDates.Clear();
                foreach (var date in toKeep)
                    calSender.SelectedDates.Add(date);
                return;
            }

            if (selectedDates.Count < 1)
            {
                _modifySelectedStart = null;
                _modifySelectedEnd = null;
                return;
            }

            var sorted = selectedDates
                .Select(d => d.DateTime)
                .OrderBy(d => d)
                .ToList();

            _modifySelectedStart = sorted[0];
            _modifySelectedEnd = sorted[sorted.Count - 1];

            // force redraw
            var minDate = calSender.MinDate;
            calSender.MinDate = DateTimeOffset.Now.Date.AddDays(1);
            calSender.MinDate = minDate;
        };

        calendar.SelectedDates.Add(vm.SelectedTimeRange.StartTime);
        if (vm.SelectedTimeRange.EndTime != vm.SelectedTimeRange.StartTime)
            calendar.SelectedDates.Add(vm.SelectedTimeRange.EndTime);

        var dialog = new ContentDialog
        {
            Title = "Modify dates",
            Content = calendar,
            PrimaryButtonText = "Confirm",
            CloseButtonText = "Cancel",
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var selectedDates = calendar.SelectedDates;
            if (selectedDates.Count < 1)
                return;

            var sorted = selectedDates
                .Select(d => d.DateTime)
                .OrderBy(d => d)
                .ToList();

            var newRange = new TimeRange(sorted[0], sorted[sorted.Count - 1]);
            vm.UpdateSelectedRange(newRange);
        }
    }

    private void OnConfirmClicked(object sender, RoutedEventArgs e)
    {
        var vm = (ConfirmBookingViewModel)this.DataContext;
        vm.ConfirmBooking();
    }

    private void OnMessageUserClicked(object sender, RoutedEventArgs e)
    {
        // to be connected later
    }
}