using System.Collections.Generic;
using System.Threading.Tasks;
using OurApp.Core.Models;

namespace OurApp.Core.Services
{
    public interface IPaymentService
    {
        Task<string> ProcessPaymentAsync(int jobId, int amount, string name, string cardNum, string exp, string cvv);
        List<JobPaymentInfo> GetPaidJobsInfo(string jobType, string expLevel);
    }
}