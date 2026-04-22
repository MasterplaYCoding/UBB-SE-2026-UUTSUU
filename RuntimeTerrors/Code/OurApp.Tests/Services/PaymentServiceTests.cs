using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OurApp.Core.Models;
using OurApp.Core.Services;
using OurApp.Core.Validators;
using OurApp.Tests.Helpers;


namespace OurApp.Tests.Services
{
    [TestClass]
    public class PaymentServiceTests
    {
        private FakePaymentRepository _fakeRepository = null;
        private IPaymentValidator _validator = null!;
        private PaymentService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _fakeRepository = new FakePaymentRepository();
            _validator = new PaymentValidator();
            _service = new PaymentService(_fakeRepository, _validator);
        }

        [TestMethod]
        public async Task ProcessPaymentAsync_InvalidCardHolderName_ReturnsValidationError()
        {
            var result = await _service.ProcessPaymentAsync(90101, 200, "", "123456789012345", "12/99", "123");

            Assert.AreEqual("Card Holder Name is required.", result);
            Assert.IsFalse(_fakeRepository.UpdateCalled);
        }

        [TestMethod]
        public async Task ProcessPaymentAsync_ValidInput_UpdatesRepositoryAndReturnsEmptyString()
        {
            var result = await _service.ProcessPaymentAsync(90101, 200, "John Doe", "123456789012345", "12/99", "123");

            Assert.AreEqual(string.Empty, result);
            Assert.IsTrue(_fakeRepository.UpdateCalled);
            Assert.AreEqual(90101, _fakeRepository.LastUpdatedJobId);
            Assert.AreEqual(200, _fakeRepository.LastUpdatedAmount);
        }

        [TestMethod]
        public async Task ProcessPaymentAsync_WhenRepositoryThrows_ReturnsDatabaseError()
        {
            _fakeRepository.ThrowOnUpdate = true;

            var result = await _service.ProcessPaymentAsync(90101, 200, "John Doe", "123456789012345", "12/99", "123");

            Assert.AreEqual("Database Error: boom", result);
        }

        [TestMethod]
        public void GetPaidJobsInfo_ReturnsRepositoryData()
        {
            _fakeRepository.PaidJobsToReturn.Add(new JobPaymentInfo
            {
                CompanyName = "Budget Company",
                JobTitle = "Backend Developer",
                AmountPayed = 250
            });

            var result = _service.GetPaidJobsInfo("Full-time", "Entry Level");

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(string.Equals(
                "Budget Company",
                result[0].CompanyName?.Trim(),
                StringComparison.Ordinal));
            Assert.IsTrue(string.Equals(
                "Backend Developer",
                result[0].JobTitle?.Trim(),
                StringComparison.Ordinal));
            Assert.AreEqual(250, result[0].AmountPayed);
        }


        [TestMethod]
        public void GetPaidJobsInfo_ReturnsSameNumberOfItemsAsRepository()
        {
            _fakeRepository.PaidJobsToReturn.Add(new JobPaymentInfo
            {
                CompanyName = "Company A",
                JobTitle = "Job A",
                AmountPayed = 100
            });
            _fakeRepository.PaidJobsToReturn.Add(new JobPaymentInfo
            {
                CompanyName = "Company B",
                JobTitle = "Job B",
                AmountPayed = 200
            });

            var result = _service.GetPaidJobsInfo("Full-time", "Entry Level");

            Assert.AreEqual(2, result.Count);
        }

    }
}
