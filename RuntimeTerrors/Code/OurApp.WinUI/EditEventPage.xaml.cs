using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using OurApp.Core.Models;
using OurApp.Core.Validators;
using OurApp.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OurApp.WinUI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditEventPage : Page
    {
        private bool StartDateModified = false;
        private bool EndDateModified = false;
        private bool IsLoaded = false;
        public EditEventViewModel ViewModel { get; set; }

        /// <summary>
        /// Edit event page constructor
        /// </summary>
        public EditEventPage()
        {
            InitializeComponent();

            IsLoaded = true;
        }

        /// <summary>
        /// Function that initializes the edit event view model, only when the user
        /// gets navigated to this page. The selected event is sent as a parameter
        /// to the view model
        /// </summary>
        /// <param name="e"> selected event to edit </param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var ev = e.Parameter as Event;

            var mainW = App.mainWindow;
            ViewModel = new EditEventViewModel(mainW.eventsService, ev, mainW.eventValidator);
            this.DataContext = ViewModel;

            System.Diagnostics.Debug.WriteLine(ev);
        }


        /// <summary>
        /// Function that displays an appropriate ContentDialog, based on the success/
        /// failure of editing an existing event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SaveEvent_Click(object sender, RoutedEventArgs e)
        {
            // because click usually runs before command so we must make it run before
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
                    Title = "YEY!",
                    Content = "Event saved successfully!",
                    CloseButtonText = "Close",
                    XamlRoot = this.XamlRoot
                };
            }
            else
            {
                popup = new ContentDialog
                {
                    Title = "Oops!",
                    Content = "We’re sorry, an error occurred. The event was not saved. Please try again.",
                    CloseButtonText = "Close",
                    XamlRoot = this.XamlRoot
                };
            }

            await popup.ShowAsync();
        }


        /// <summary>
        /// Function that takes the user back to the "Our events" page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavigateBack_Click(object sender, RoutedEventArgs e)
        {
            var mainW = App.mainWindow;
            mainW.RootFrame.Navigate(typeof(OurEventsPage));
        }


        /// <summary>
        /// Function that controls the border colour of the title textbox based on 
        /// its valid state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Function that controls the border colour of the description textbox based on 
        /// its valid state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Function that controls the border colour of the start date picker based on 
        /// its valid state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void StartDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (!IsLoaded)
                return;

            if (!StartDateModified)
            {
                StartDateModified = true;
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


        /// <summary>
        /// Function that controls the border colour of the end date picker based on 
        /// its valid state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void EndDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (!IsLoaded)
                return;

            if (!EndDateModified)
            {
                EndDateModified = true;
                return;
            }

            if (StartDateModified)
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

        /// <summary>
        /// Function that controls the border colour of the location textbox based on 
        /// its valid state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Function that displays a ContentDialog if the user tries to press the "Cancel"
        /// button. The ContentDialog shows 2 buttons: Yes and No. If the chosen button
        /// was "Yes", the user is taken back to the "Our Events" page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CancelChanges_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Confirm cancel",
                Content = "Are you sure you want to cancel the modifications?",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                NavigateBack_Click(sender, e);
            }
        }

        /// <summary>
        /// Function that displays a ContentDialog if the user tries to press the "Delete"
        /// button. The ContentDialog shows 2 buttons: Yes and No. If the chosen button
        /// was "Yes", the user is taken back to the "Our Events" page and the event gets 
        /// deleted and an appropriate ContentDialog gets displayed, based on the success/
        /// failure of deleting the event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DeleteEvent_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Confirm delete",
                Content = "Are you sure you want to delete the event?",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No",
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
                    Title = "YEY!",
                    Content = "Event deleted successfully!",
                    CloseButtonText = "Close",
                    XamlRoot = this.XamlRoot
                };
            }
            else
            {
                popup = new ContentDialog
                {
                    Title = "Oops!",
                    Content = "We’re sorry, an error occurred. The event was not deleted. Please try again.",
                    CloseButtonText = "Close",
                    XamlRoot = this.XamlRoot
                };
            }

            await popup.ShowAsync();
        }
    }
}
