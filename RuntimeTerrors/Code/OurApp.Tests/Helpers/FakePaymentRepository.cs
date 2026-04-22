using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OurApp.Core.Models;
using OurApp.Core.Repositories;

namespace OurApp.Tests.Helpers
{
    internal class FakePaymentRepository : IPaymentRepository
    {
        public int LastUpdatedJobId { get; private set; }
        public int LastUpdatedAmount { get; private set; }
        public bool UpdateCalled { get; private set; }
        public bool ThrowOnUpdate { get; set; }

        public List<JobPaymentInfo> PaidJobsToReturn { get; } = new();
        public List<string> EmailsToNotify { get; } = new();

        public void UpdateJobPayment(int jobId, int paymentAmount)
        {
            if (ThrowOnUpdate)
            {
                throw new Exception("boom");
            }

            UpdateCalled = true;
            LastUpdatedJobId = jobId;
            LastUpdatedAmount = paymentAmount;
        }


        public List<JobPaymentInfo> GetPaidJobs(string jobType, string experienceLevel)
        {
            return PaidJobsToReturn;
        }

        public List<string> GetCompaniesToNotify(int currentJobId, int newPaymentAmount)
        {
            return EmailsToNotify;
        }


    }
}
