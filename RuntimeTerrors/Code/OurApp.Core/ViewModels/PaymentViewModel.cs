using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OurApp.Core.Models;
using OurApp.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace OurApp.Core.ViewModels
{
    public partial class PaymentViewModel : ObservableObject
    {
        private const string DefaultJobTypeValue = "Full-Time";
        private const string DefaultExperienceLevelValue = "Senior";
        private const int MinimumValidPaymentAmount = 0;

        private const string MessageTitleInvalidAmount = "Invalid Amount";
        private const string MessageBodyInvalidAmount = "Please enter a valid numerical amount greater than 0.";
        private const string MessageTitleError = "Error";
        private const string MessageTitleSuccess = "Success";
        private const string MessageBodySuccessPrefix = "Payment of $";
        private const string MessageBodySuccessSuffix = " processed successfully. Emails dispatched!";

        private readonly IPaymentService _paymentService;

        [ObservableProperty] private string _cardHolderName = string.Empty;
        [ObservableProperty] private string _cardNumber = string.Empty;
        [ObservableProperty] private string _expDate = string.Empty;
        [ObservableProperty] private string _cvv = string.Empty;
        [ObservableProperty] private string _amountToPayText = string.Empty;

        [ObservableProperty] private string _selectedJobType = DefaultJobTypeValue;
        [ObservableProperty] private string _selectedExperienceLevel = DefaultExperienceLevelValue;

        public ObservableCollection<JobPaymentInfo> PaymentData { get; } = new ObservableCollection<JobPaymentInfo>();

        public Action<string, string>? ShowMessageAction { get; set; }
        public Action? CloseWindowAction { get; set; }
        public int CurrentJobId { get; set; }

        public PaymentViewModel(IPaymentService paymentService)
        {
            _paymentService = paymentService;
            LoadData(); // Load initially
        }

        partial void OnSelectedJobTypeChanged(string value) => LoadData();
        partial void OnSelectedExperienceLevelChanged(string value) => LoadData();

        private void LoadData()
        {
            if (string.IsNullOrEmpty(SelectedJobType) || string.IsNullOrEmpty(SelectedExperienceLevel)) return;

            PaymentData.Clear();
            var dataFromDatabase = _paymentService.GetPaidJobsInfo(SelectedJobType, SelectedExperienceLevel);

            foreach (var item in dataFromDatabase)
            {
                PaymentData.Add(item);
            }
        }

        [RelayCommand]
        private async Task Pay()
        {
            if (!int.TryParse(AmountToPayText, out int amountToPay) || amountToPay <= MinimumValidPaymentAmount)
            {
                ShowMessageAction?.Invoke(MessageTitleInvalidAmount, MessageBodyInvalidAmount);
                return;
            }

            string resultMessage = await _paymentService.ProcessPaymentAsync(CurrentJobId, amountToPay, CardHolderName, CardNumber, ExpDate, Cvv);

            if (!string.IsNullOrEmpty(resultMessage))
            {
                ShowMessageAction?.Invoke(MessageTitleError, resultMessage);
            }
            else
            {
                ShowMessageAction?.Invoke(MessageTitleSuccess, $"{MessageBodySuccessPrefix}{amountToPay}{MessageBodySuccessSuffix}");
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