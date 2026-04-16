using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using OurApp.Core.Models;
using OurApp.WinUI.ViewModels;

namespace OurApp.WinUI
{
    public sealed partial class OurJobsPage : Page
    {
        public OurJobsViewModel ViewModel { get; }

        public OurJobsPage()
        {
            this.InitializeComponent();
            ViewModel = new OurJobsViewModel();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.Back)
            {
                ViewModel.ReloadJobs();
            }
        }

        private void CreateJobButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CreateJobPage));
        }

        private void ViewApplicantsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is JobPosting job)
            {
                Frame.Navigate(typeof(JobApplicantsPage), job);
            }
        }

        private void PayToPromoteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is JobPosting job)
            {
                var paymentWindow = new PaymentWindow(job.JobId);
                paymentWindow.Activate();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }
    }
}
