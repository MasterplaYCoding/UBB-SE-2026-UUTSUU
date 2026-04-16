using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using OurApp.Core.Models;
using OurApp.WinUI.ViewModels;

namespace OurApp.WinUI
{
    public sealed partial class JobApplicantsPage : Page
    {
        public JobApplicantsViewModel ViewModel { get; private set; }

        public JobApplicantsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is JobPosting job)
            {
                ViewModel = new JobApplicantsViewModel(job, App.mainWindow?.sessionService);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.TableVisibility == Visibility.Collapsed)
            {
                ViewModel.GoBackFromDetails();
            }
            else if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void ApplicantsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = sender as ListView;
            if (listView?.SelectedItem is Applicant applicant)
            {
                ViewModel.SelectedApplicant = applicant;
            }
        }

        private void RemoveApplicant_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Applicant applicant)
            {
                ViewModel.RemoveApplicant(applicant);
            }
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SaveChanges();
        }

        private async void ScanCv_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.ScanCvAsync();
            }
        }

        private async void SendStatusMail_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }

            var (ok, message) = await ViewModel.SendStatusMailAsync();
            var dialog = new ContentDialog
            {
                Title = ok ? "Email sent" : "Could not send email",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }
    }

    public class EmptyStringToPendingConverter : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var str = value as string;
            return string.IsNullOrWhiteSpace(str) ? "Pending" : str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
