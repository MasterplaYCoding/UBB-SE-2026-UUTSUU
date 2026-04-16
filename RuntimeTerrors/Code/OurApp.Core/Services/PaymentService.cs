using OurApp.Core.Database;
using OurApp.Core.Models;
using OurApp.Core.Repositories;
using OurApp.Core.Validators;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace OurApp.Core.Services
{
    public class PaymentService
    {
        private readonly PaymentValidator _validator = new PaymentValidator();
        private readonly PaymentRepository _repository = new PaymentRepository();

        public async Task<string> ProcessPaymentAsync(int jobId, int amount, string name, string cardNum, string exp, string cvv)
        {
            string validationError = _validator.Validate(name, cardNum, exp, cvv);
            if (!string.IsNullOrEmpty(validationError)) return validationError;

            try
            {
                // 1. Save to database
                _repository.UpdateJobPayment(jobId, amount);

                // 2. Fetch emails to notify
                List<string> emailsToNotify = _repository.GetCompaniesToNotify(jobId, amount);

                // 3. Send Emails 
                if (emailsToNotify.Count > 0)
                {
                    await SendNotificationEmailsAsync(emailsToNotify, amount);
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                return $"Database Error: {ex.Message}";
            }
        }

        private async Task SendNotificationEmailsAsync(List<string> emails, int newAmount)
        {
            try
            {
                var fromAddress = new MailAddress("carla.draghiciu@cnglsibiu.ro", "Job Portal Admin");
                const string fromPassword = "angxokbiqoyodwgm";

                using (var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                    Timeout = 60000
                })
                {
                    foreach (string email in emails)
                    {
                        var toAddress = new MailAddress(email);
                        string subject = "Job Promotion Alert!";
                        string body = $"Hello, \n\nJust letting you know that a competitor has placed a bid of ${newAmount} on a job that shares the same Type and Experience Level as yours. Consider increasing your budget to stay competitive!";

                        using (var message = new MailMessage(fromAddress, toAddress)
                        {
                            Subject = subject,
                            Body = body
                        })
                        {
                            await smtp.SendMailAsync(message);
                            System.Diagnostics.Debug.WriteLine($"Email sent to {email}!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to send email: {ex.Message}");
            }
        }

        public List<JobPaymentInfo> GetPaidJobsInfo(string jobType, string expLevel)
        {
            return _repository.GetPaidJobs(jobType, expLevel);
        }
    }
}