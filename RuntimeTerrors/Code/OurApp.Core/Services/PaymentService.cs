using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using OurApp.Core.Database;
using OurApp.Core.Models;
using OurApp.Core.Repositories;
using OurApp.Core.Validators;

namespace OurApp.Core.Services
{
    public class PaymentService : IPaymentService
    {
        private const int EmptyCollectionCount = 0;
        private const string AdminEmailAddress = "carla.draghiciu@cnglsibiu.ro";
        private const string AdminEmailDisplayName = "Job Portal Admin";
        private const string AdminEmailPassword = "[REDACTED_PASSWORD]";
        private const string SmtpHostAddress = "smtp.gmail.com";
        private const int SmtpHostPort = 587;
        private const int SmtpTimeoutMilliseconds = 60000;
        private const string NotificationEmailSubject = "Job Promotion Alert!";
        private const string DatabaseErrorMessagePrefix = "Database Error: ";
        private const string EmailSentDebugMessagePrefix = "Email sent to ";
        private const string EmailFailedDebugMessagePrefix = "Failed to send email: ";

        private readonly IPaymentValidator validator;
        private readonly IPaymentRepository repository;

        public PaymentService(IPaymentRepository repository, IPaymentValidator paymentValidator)
        {
            this.repository = repository;
            validator = paymentValidator;
        }

        public async Task<string> ProcessPaymentAsync(int jobId, int amount, string name, string cardNum, string exp, string cvv)
        {
            string validationError = validator.ValidatePaymentDetails(name, cardNum, exp, cvv);
            if (!string.IsNullOrEmpty(validationError))
            {
                return validationError;
            }
            try
            {
                // 1. Save to database
                repository.UpdateJobPayment(jobId, amount);

                // 2. Fetch emails to notify
                List<string> emailsToNotify = repository.GetCompaniesToNotify(jobId, amount);

                // 3. Send Emails
                if (emailsToNotify.Count > EmptyCollectionCount)
                {
                    await SendNotificationEmailsAsync(emailsToNotify, amount);
                }

                return string.Empty;
            }
            catch (Exception exception)
            {
                return $"{DatabaseErrorMessagePrefix}{exception.Message}";
            }
        }

        private async Task SendNotificationEmailsAsync(List<string> emails, int newAmount)
        {
            try
            {
                var fromAddress = new MailAddress(AdminEmailAddress, AdminEmailDisplayName);

                using (var smtpClient = new SmtpClient
                {
                    Host = SmtpHostAddress,
                    Port = SmtpHostPort,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(fromAddress.Address, AdminEmailPassword),
                    Timeout = SmtpTimeoutMilliseconds
                })
                {
                    foreach (string email in emails)
                    {
                        var toAddress = new MailAddress(email);
                        string notificationBody = $"Hello, \n\nJust letting you know that a competitor has placed a bid of ${newAmount} on a job that shares the same Type and Experience Level as yours. Consider increasing your budget to stay competitive!";

                        using (var mailMessage = new MailMessage(fromAddress, toAddress)
                        {
                            Subject = NotificationEmailSubject,
                            Body = notificationBody
                        })
                        {
                            await smtpClient.SendMailAsync(mailMessage);
                            System.Diagnostics.Debug.WriteLine($"{EmailSentDebugMessagePrefix}{email}!");
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"{EmailFailedDebugMessagePrefix}{exception.Message}");
            }
        }

        public List<JobPaymentInfo> GetPaidJobsInfo(string jobType, string expLevel)
        {
            return repository.GetPaidJobs(jobType, expLevel);
        }
    }
}