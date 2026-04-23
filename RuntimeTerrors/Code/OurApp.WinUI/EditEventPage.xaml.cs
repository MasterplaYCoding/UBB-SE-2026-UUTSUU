using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media;
using OurApp.Core.Models;
using OurApp.Core.ViewModels;
using System;

namespace OurApp.WinUI
{
    public sealed partial class EditEventPage : Page
    {
        private const string DialogTitleSuccess = "YEY!";
        private const string DialogContentSaveSuccess = "Event saved successfully!";
        private const string DialogContentDeleteSuccess = "Event deleted successfully!";
        private const string DialogTitleError = "Oops!";
        private const string DialogContentSaveError = "We’re sorry, an error occurred. The event was not saved. Please try again.";
        private const string DialogContentDeleteError = "We’re sorry, an error occurred. The event was not deleted. Please try again.";
        private const string DialogButtonClose = "Close";

        private const string DialogTitleConfirmCancel = "Confirm cancel";
        private const string DialogContentConfirmCancel = "Are you sure you want to cancel the modifications?";
        private const string DialogTitleConfirmDelete = "Confirm delete";
        private const string DialogContentConfirmDelete = "Are you sure you want to delete the event?";
        private const string DialogButtonYes = "Yes";
        private const string DialogButtonNo = "No";

        private bool _startDateModified = false;
        private bool _endDateModified = false;
        private bool _isLoaded = false;

        public EditEventViewModel ViewModel { get; set; }

        public EditEventPage()
        {
            InitializeComponent();
            _isLoaded = true;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var eventToEdit = e.Parameter as Event;
            var mainWindow = App.mainWindow;

            ViewModel = new EditEventViewModel(mainWindow.eventsService, eventToEdit, mainWindow.eventValidator);
            this.DataContext = ViewModel;

            System.Diagnostics.Debug.WriteLine(eventToEdit);
        }

        private async void SaveEvent_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.EditEventCommand.Execute(null);

            if (ViewModel.isEverythingValid)
            {
                NavigateBack_Click(sender, e);
            }
            else
            {
                return;
            }

            ContentDialog popup;
            if (ViewModel.eventUpdatedSuccessfully)
            {
                popup = new ContentDialog
                {
                    Title = DialogTitleSuccess,
                    Content = DialogContentSaveSuccess,
                    CloseButtonText = DialogButtonClose,
                    XamlRoot = this.XamlRoot
                };
            }
            else
            {
                popup = new ContentDialog
                {
                    Title = DialogTitleError,
                    Content = DialogContentSaveError,
                    CloseButtonText = DialogButtonClose,
                    XamlRoot = this.XamlRoot
                };
            }

            await popup.ShowAsync();
        }

        private void NavigateBack_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = App.mainWindow;
            mainWindow.RootFrame.Navigate(typeof(OurEventsPage));
        }

        private void Title_LostFocus(object sender, RoutedEventArgs e)
        {
            var binding = TitleBox.GetBindingExpression(TextBox.TextProperty);
            binding?.UpdateSource();

            if (ViewModel.ValidateTitle())
            {
                TitleBox.BorderBrush = new SolidColorBrush(Colors.Green);
            }
            else
            {
                TitleBox.BorderBrush = new SolidColorBrush(Colors.Red);
            }
        }

        private void Description_LostFocus(object sender, RoutedEventArgs e)
        {
            var binding = DescriptionBox.GetBindingExpression(TextBox.TextProperty);
            binding?.UpdateSource();

            if (ViewModel.ValidateDescription())
            {
                DescriptionBox.BorderBrush = new SolidColorBrush(Colors.Green);
            }
            else
            {
                DescriptionBox.BorderBrush = new SolidColorBrush(Colors.Red);
            }
        }

        private void StartDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (!_isLoaded)
                return;

            if (!_startDateModified)
            {
                _startDateModified = true;
                return;
            }

            if (ViewModel.ValidateDatesCronologity())
            {
                StartDatePicker.BorderBrush = new SolidColorBrush(Colors.Green);
            }
            else
            {
                StartDatePicker.BorderBrush = new SolidColorBrush(Colors.Red);
            }
        }

        private void EndDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (!_isLoaded)
                return;

            if (!_endDateModified)
            {
                _endDateModified = true;
                return;
            }

            if (_startDateModified)
            {
                if (ViewModel.ValidateDatesCronologity())
                {
                    EndDatePicker.BorderBrush = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    EndDatePicker.BorderBrush = new SolidColorBrush(Colors.Red);
                }
            }
        }

        private void Location_LostFocus(object sender, RoutedEventArgs e)
        {
            var binding = LocationBox.GetBindingExpression(TextBox.TextProperty);
            binding?.UpdateSource();

            if (ViewModel.ValidateLocation())
            {
                LocationBox.BorderBrush = new SolidColorBrush(Colors.Green);
            }
            else
            {
                LocationBox.BorderBrush = new SolidColorBrush(Colors.Red);
            }
        }

        private async void CancelChanges_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = DialogTitleConfirmCancel,
                Content = DialogContentConfirmCancel,
                PrimaryButtonText = DialogButtonYes,
                CloseButtonText = DialogButtonNo,
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                NavigateBack_Click(sender, e);
            }
        }

        private async void DeleteEvent_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = DialogTitleConfirmDelete,
                Content = DialogContentConfirmDelete,
                PrimaryButtonText = DialogButtonYes,
                CloseButtonText = DialogButtonNo,
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                ViewModel.DeleteEventCommand.Execute(null);
                NavigateBack_Click(sender, e);
            }
            else
            {
                return;
            }

            ContentDialog popup;
            if (ViewModel.eventDeletedSuccessfully)
            {
                popup = new ContentDialog
                {
                    Title = DialogTitleSuccess,
                    Content = DialogContentDeleteSuccess,
                    CloseButtonText = DialogButtonClose,
                    XamlRoot = this.XamlRoot
                };
            }
            else
            {
                popup = new ContentDialog
                {
                    Title = DialogTitleError,
                    Content = DialogContentDeleteError,
                    CloseButtonText = DialogButtonClose,
                    XamlRoot = this.XamlRoot
                };
            }

            await popup.ShowAsync();
        }
    }
}
