using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OurApp.Core.Validators;

namespace OurApp.Tests.Validators
{
    [TestClass]
    public class PaymentValidatorTests
    {
        private PaymentValidator _validator = null!;

        [TestInitialize]
        public void Setup()
        {
            _validator = new PaymentValidator();
        }

        [TestMethod]
        public void ValidatePaymentDetails_EmptyName_ReturnsError()
        {
            var result = _validator.ValidatePaymentDetails("", "123456789012345", "12/99", "123");

            Assert.AreEqual("Card Holder Name is required.", result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_WhitespaceName_ReturnsError()
        {
            var result = _validator.ValidatePaymentDetails("   ", "123456789012345", "12/99", "123");

            Assert.AreEqual("Card Holder Name is required.", result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_EmptyCardNumber_ReturnsError()
        {
            var result = _validator.ValidatePaymentDetails("John Doe", "", "12/99", "123");

            Assert.AreEqual("Please enter a valid Card Number.", result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_WhitespaceCardNumber_ReturnsError()
        {
            var result = _validator.ValidatePaymentDetails("John Doe", "   ", "12/99", "123");

            Assert.AreEqual("Please enter a valid Card Number.", result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_ShortCardNumber_ReturnsError()
        {
            var result = _validator.ValidatePaymentDetails("John Doe", "123", "12/99", "123");

            Assert.AreEqual("Please enter a valid Card Number.", result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_EmptyExpirationDate_ReturnsError()
        {
            var result = _validator.ValidatePaymentDetails("John Doe", "123456789012345", "", "123");

            Assert.AreEqual("Expiration Date must be in MM/YY format.", result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_WhitespaceExpirationDate_ReturnsError()
        {
            var result = _validator.ValidatePaymentDetails("John Doe", "123456789012345", "   ", "123");

            Assert.AreEqual("Expiration Date must be in MM/YY format.", result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_ExpirationWithoutSlash_ReturnsError()
        {
            var result = _validator.ValidatePaymentDetails("John Doe", "123456789012345", "1299", "123");

            Assert.AreEqual("Expiration Date must be in MM/YY format.", result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_ExpirationWithInvalidNumbers_ReturnsError()
        {
            var result = _validator.ValidatePaymentDetails("John Doe", "123456789012345", "ab/cd", "123");

            Assert.AreEqual("Expiration Date must contain valid numbers (MM/YY).", result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_ExpirationWithMissingYearPart_ReturnsError()
        {
            var result = _validator.ValidatePaymentDetails("John Doe", "123456789012345", "12/", "123");

            Assert.AreEqual("Expiration Date must contain valid numbers (MM/YY).", result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_MonthBelowRange_ReturnsError()
        {
            var result = _validator.ValidatePaymentDetails("John Doe", "123456789012345", "00/99", "123");

            Assert.AreEqual("Invalid expiration month. Must be between 01 and 12.", result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_MonthAboveRange_ReturnsError()
        {
            var result = _validator.ValidatePaymentDetails("John Doe", "123456789012345", "13/99", "123");

            Assert.AreEqual("Invalid expiration month. Must be between 01 and 12.", result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_EmptyCvv_ReturnsError()
        {
            var result = _validator.ValidatePaymentDetails("John Doe", "123456789012345", "12/99", "");

            Assert.AreEqual("Please enter a valid CVV.", result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_WhitespaceCvv_ReturnsError()
        {
            var result = _validator.ValidatePaymentDetails("John Doe", "123456789012345", "12/99", "   ");

            Assert.AreEqual("Please enter a valid CVV.", result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_ShortCvv_ReturnsError()
        {
            var result = _validator.ValidatePaymentDetails("John Doe", "123456789012345", "12/99", "12");

            Assert.AreEqual("Please enter a valid CVV.", result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_PreviousMonthInCurrentYear_ReturnsExpiredError()
        {
            var today = DateTime.Now;
            var previousMonth = today.Month == 1 ? 12 : today.Month - 1;
            var year = today.Month == 1 ? today.Year - 1 : today.Year;
            var yy = (year - 2000).ToString("00");
            var exp = $"{previousMonth:00}/{yy}";

            var result = _validator.ValidatePaymentDetails("John Doe", "123456789012345", exp, "123");

            Assert.AreEqual("This card has expired. Please use a valid card.", result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_PreviousYear_ReturnsExpiredError()
        {
            var yy = ((DateTime.Now.Year - 1) - 2000).ToString("00");

            var result = _validator.ValidatePaymentDetails("John Doe", "123456789012345", $"12/{yy}", "123");

            Assert.AreEqual("This card has expired. Please use a valid card.", result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_CurrentMonthAndYear_ReturnsEmptyString()
        {
            var today = DateTime.Now;
            var exp = $"{today.Month:00}/{(today.Year - 2000):00}";

            var result = _validator.ValidatePaymentDetails("John Doe", "123456789012345", exp, "123");

            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_FutureDate_ReturnsEmptyString()
        {
            var result = _validator.ValidatePaymentDetails("John Doe", "123456789012345", "12/99", "123");

            Assert.AreEqual(string.Empty, result);
        }
    }
}
