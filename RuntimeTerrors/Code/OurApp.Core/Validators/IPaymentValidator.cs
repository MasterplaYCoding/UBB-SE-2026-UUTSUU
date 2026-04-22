namespace OurApp.Core.Validators
{
    public interface IPaymentValidator
    {
        string ValidatePaymentDetails(string cardHolderName, string cardNumber, string expirationDate, string cardVerificationValue);
    }
}