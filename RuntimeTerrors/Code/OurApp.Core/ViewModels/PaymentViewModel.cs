using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OurApp.Core.Models;
using OurApp.Core.Services;
using System;
using System.Collections.ObjectModel;

namespace OurApp.Core.ViewModels
{
    public partial class PaymentViewModel : ObservableObject
    {
        private readonly PaymentService _paymentService = new PaymentService();
        [ObservableProperty] private string _cardHolderName;
        [ObservableProperty] private string _cardNumber;
        [ObservableProperty] private string _expDate;
        [ObservableProperty] private string _cvv;
        [ObservableProperty] private string _amountToPayText;

        [ObservableProperty] private string _selectedJobType = "Full-Time";
        [ObservableProperty] private string _selectedExperienceLevel = "Senior";

        public ObservableCollection<JobPaymentInfo> PaymentData { get; } = new ObservableCollection<JobPaymentInfo>();

        public Action<string, string> ShowMessageAction { get; set; }
        public Action CloseWindowAction { get; set; }
        public int CurrentJobId { get; set; }

        public PaymentViewModel()
        {
            LoadData(); // Load initially
        }

        partial void OnSelectedJobTypeChanged(string value) => LoadData();
        partial void OnSelectedExperienceLevelChanged(string value) => LoadData();

        private void LoadData()
        {
            if (string.IsNullOrEmpty(SelectedJobType) || string.IsNullOrEmpty(SelectedExperienceLevel)) return;

            PaymentData.Clear();
            var dataFromDb = _paymentService.GetPaidJobsInfo(SelectedJobType, SelectedExperienceLevel);

            foreach (var item in dataFromDb)
            {
                PaymentData.Add(item);
            }
        }

        [RelayCommand]
        private async Task Pay() 
        {
            if (!int.TryParse(AmountToPayText, out int amountToPay) || amountToPay <= 0)
            {
                ShowMessageAction?.Invoke("Invalid Amount", "Please enter a valid numerical amount greater than 0.");
                return;
            }

            string resultMessage = await _paymentService.ProcessPaymentAsync(CurrentJobId, amountToPay, CardHolderName, CardNumber, ExpDate, Cvv);

            if (!string.IsNullOrEmpty(resultMessage))
            {
                ShowMessageAction?.Invoke("Error", resultMessage);
            }
            else
            {
                ShowMessageAction?.Invoke("Success", $"Payment of ${amountToPay} processed successfully. Emails dispatched!");
                LoadData();
                AmountToPayText = string.Empty;
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseWindowAction?.Invoke();
        }
    }
}