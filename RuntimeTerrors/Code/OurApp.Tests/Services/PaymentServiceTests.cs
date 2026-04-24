using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OurApp.Core.Models;
using OurApp.Core.Services;
using OurApp.Core.Validators;
using OurApp.Tests.Helpers;

namespace OurApp.Tests.Services
{
    [TestClass]
    public class PaymentServiceTests
    {
        private const int ValidJobId = 90101;
        private const int ValidAmount = 200;
        private const int Amount250 = 250;
        private const int Amount100 = 100;

        private const int CountOne = 1;
        private const int CountTwo = 2;

        private const string EmptyString = "";
        private const string ValidName = "John Doe";
        private const string ValidCard = "123456789012345";
        private const string ValidExp = "12/99";
        private const string ValidCvv = "123";

        private const string NameRequiredError = "Card Holder Name is required.";
        private const string DbErrorBoom = "Database Error: boom";

        private const string JobTypeFullTime = "Full-time";
        private const string ExpLevelEntry = "Entry Level";

        private const string CompanyBudget = "Budget Company";
        private const string CompanyA = "Company A";
        private const string CompanyB = "Company B";

        private const string TitleBackend = "Backend Developer";
        private const string TitleJobA = "Job A";
        private const string TitleJobB = "Job B";

        private const string NotifyEmail = "test@competitor.com";

        private FakePaymentRepository fakeRepository = null!;
        private IPaymentValidator validator = null!;
        private PaymentService service = null!;

        [TestInitialize]
        public void Setup()
        {
            fakeRepository = new FakePaymentRepository();
            validator = new PaymentValidator();
            service = new PaymentService(fakeRepository, validator);
        }

        [TestMethod]
        public async Task ProcessPaymentAsync_InvalidCardHolderName_ReturnsValidationError()
        {
            var result = await service.ProcessPaymentAsync(ValidJobId, ValidAmount, EmptyString, ValidCard, ValidExp, ValidCvv);

            Assert.AreEqual(NameRequiredError, result);
            Assert.IsFalse(fakeRepository.UpdateCalled);
        }

        [TestMethod]
        public async Task ProcessPaymentAsync_ValidInput_UpdatesRepositoryAndReturnsEmptyString()
        {
            var result = await service.ProcessPaymentAsync(ValidJobId, ValidAmount, ValidName, ValidCard, ValidExp, ValidCvv);

            Assert.AreEqual(EmptyString, result);
            Assert.IsTrue(fakeRepository.UpdateCalled);
            Assert.AreEqual(ValidJobId, fakeRepository.LastUpdatedJobId);
            Assert.AreEqual(ValidAmount, fakeRepository.LastUpdatedAmount);
        }

        [TestMethod]
        public async Task ProcessPaymentAsync_WhenRepositoryThrows_ReturnsDatabaseError()
        {
            fakeRepository.ThrowOnUpdate = true;

            var result = await service.ProcessPaymentAsync(ValidJobId, ValidAmount, ValidName, ValidCard, ValidExp, ValidCvv);

            Assert.AreEqual(DbErrorBoom, result);
        }

        [TestMethod]
        public async Task ProcessPaymentAsync_WithEmailsToNotify_HitsSmtpCatchBlockAndReturnsEmptyString()
        {
            fakeRepository.EmailsToNotify.Add(NotifyEmail);

            var result = await service.ProcessPaymentAsync(ValidJobId, ValidAmount, ValidName, ValidCard, ValidExp, ValidCvv);

            Assert.AreEqual(EmptyString, result);
            Assert.IsTrue(fakeRepository.UpdateCalled);
        }

        [TestMethod]
        public void GetPaidJobsInfo_ReturnsRepositoryData()
        {
            fakeRepository.PaidJobsToReturn.Add(new JobPaymentInfo
            {
                CompanyName = CompanyBudget,
                JobTitle = TitleBackend,
                AmountPayed = Amount250
            });

            var result = service.GetPaidJobsInfo(JobTypeFullTime, ExpLevelEntry);

            Assert.AreEqual(CountOne, result.Count);
            Assert.IsTrue(string.Equals(
                CompanyBudget,
                result[0].CompanyName?.Trim(),
                StringComparison.Ordinal));
            Assert.IsTrue(string.Equals(
                TitleBackend,
                result[0].JobTitle?.Trim(),
                StringComparison.Ordinal));
            Assert.AreEqual(Amount250, result[0].AmountPayed);
        }

        [TestMethod]
        public void GetPaidJobsInfo_ReturnsSameNumberOfItemsAsRepository()
        {
            fakeRepository.PaidJobsToReturn.Add(new JobPaymentInfo
            {
                CompanyName = CompanyA,
                JobTitle = TitleJobA,
                AmountPayed = Amount100
            });
            fakeRepository.PaidJobsToReturn.Add(new JobPaymentInfo
            {
                CompanyName = CompanyB,
                JobTitle = TitleJobB,
                AmountPayed = ValidAmount
            });

            var result = service.GetPaidJobsInfo(JobTypeFullTime, ExpLevelEntry);

            Assert.AreEqual(CountTwo, result.Count);
        }
    }
}