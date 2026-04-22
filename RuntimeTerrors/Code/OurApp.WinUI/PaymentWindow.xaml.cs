using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OurApp.Core.ViewModels;
using System;

namespace OurApp.WinUI
{
    public sealed partial class PaymentWindow : Window
    {
        public PaymentViewModel ViewModel { get; }

        public PaymentWindow(int jobId)
        {
            this.InitializeComponent();

            var mainWindow = App.mainWindow;
            ViewModel = new PaymentViewModel(mainWindow.paymentService);

            ViewModel.CurrentJobId = jobId;

            ViewModel.ShowMessageAction = ShowMessage;
            ViewModel.CloseWindowAction = () => this.Close();
        }

        private async void ShowMessage(string title, string content)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
