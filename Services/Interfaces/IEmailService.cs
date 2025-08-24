using SpaceFlow.ViewModels;
using System;
using System.Threading.Tasks;

namespace SpaceFlow.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendEmailConfirmationAsync(string email, string name, string confirmationCode);
        Task SendPasswordResetAsync(string email, string name, string resetCode);
        Task SendBookingConfirmationAsync(string email, string name, string workspaceName, DateTime startTime, DateTime endTime);
        Task SendBookingCancellationAsync(string email, string name, string workspaceName, DateTime startTime);
        Task SendPaymentReceiptAsync(string email, string name, decimal amount, string bookingDetails);
        Task SendWelcomeEmailAsync(string email, string name);
        Task SendContactMessageAsync(ContactFormDto contactDto);
    }
}