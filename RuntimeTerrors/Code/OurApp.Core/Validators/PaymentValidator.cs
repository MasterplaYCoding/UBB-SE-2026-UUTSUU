using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurApp.Core.Validators
{
    public class PaymentValidator
    {
        public string Validate(string name, string cardNum, string exp, string cvv)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Card Holder Name is required.";

            if (string.IsNullOrWhiteSpace(cardNum) || cardNum.Length < 15)
                return "Please enter a valid Card Number.";

            if (string.IsNullOrWhiteSpace(exp) || !exp.Contains("/"))
                return "Expiration Date must be in MM/YY format.";

            if (string.IsNullOrWhiteSpace(cvv) || cvv.Length < 3)
                return "Please enter a valid CVV.";

            var expParts = exp.Split('/');
            if (expParts.Length != 2 ||
                !int.TryParse(expParts[0], out int expMonth) ||
                !int.TryParse(expParts[1], out int expYear))
            {
                return "Expiration Date must contain valid numbers (MM/YY).";
            }

            if (expMonth < 1 || expMonth > 12)
            {
                return "Invalid expiration month. Must be between 01 and 12.";
            }

            // Convert "YY" to "YYYY"
            expYear += 2000;
            //Check if the card is expired
            DateTime currentDate = DateTime.Now;
            if (expYear < currentDate.Year || (expYear == currentDate.Year && expMonth < currentDate.Month))
            {
                return "This card has expired. Please use a valid card.";
            }

            return string.Empty;
        }
    }
}
