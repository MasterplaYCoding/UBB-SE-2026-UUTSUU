using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SearchAndBook.Shared;
using SearchAndBook.Domain;
using Windows.Gaming.Input;
using SearchAndBook.Repositories;
using SearchAndBook.Services;
using SearchAndBook.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SearchAndBook.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ConfirmBookingView : Page
{
    public ConfirmBookingView()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is not (BookingDTO BookingDTO, TimeRange range))
            return;

        var GameRepo = new GamesRepository();
        var RentalRepo = new RentalsRepository();
        var UserRepo = new UsersRepository();
        var Service = new BookingService(GameRepo, RentalRepo, UserRepo);
        var vm = new ConfirmBookingViewModel(Service, BookingDTO, range);

        vm.OnGoBackRequested += () =>
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        };

        vm.OnConfirmBookingRequested += () =>
        {
        };

        this.DataContext = vm;
    }

    private void OnBackClicked(object sender, RoutedEventArgs e)
    {
        var vm = (ConfirmBookingViewModel)this.DataContext;
        vm.GoBack();
    }

    private async void OnModifyClicked(object sender, RoutedEventArgs e)
    {
        var vm = (ConfirmBookingViewModel)this.DataContext;

        var calendar = new CalendarView
        {
            SelectionMode = CalendarViewSelectionMode.Multiple,
            HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch
        };

        calendar.CalendarViewDayItemChanging += (calSender, args) =>
        {
            var date = args.Item.Date.DateTime;
            var today = DateTime.Today;

            if (date < today)
            {
                args.Item.IsBlackout = true;
                return;
            }

            bool isUnavailable = false;
            if (vm.UnavailableTimeRanges != null)
            {
                foreach (var range in vm.UnavailableTimeRanges)
                {
                    if (date >= range.StartTime && date <= range.EndTime)
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
            }
            else
            {
                args.Item.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.DarkGreen);
            }
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

    }
}
