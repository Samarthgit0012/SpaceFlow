using SpaceFlow.Services.Interfaces;
using SpaceFlow.ViewModels;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace SpaceFlow.Services.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("EmailSettings");
                var host = smtpSettings["SmtpHost"] ?? "smtp.gmail.com";
                var port = int.Parse(smtpSettings["SmtpPort"] ?? "587");
                var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");
                var username = smtpSettings["Username"] ?? "";
                var password = smtpSettings["Password"] ?? "";
                var fromEmail = smtpSettings["FromEmail"] ?? "noreply@spaceflow.com";
                var fromName = smtpSettings["FromName"] ?? "SpaceFlow";

                using var client = new SmtpClient(host, port)
                {
                    EnableSsl = enableSsl,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(username, password)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", to);
                // In development, you might want to just log the email content
                _logger.LogInformation("Email Content - To: {To}, Subject: {Subject}, Body: {Body}", to, subject, body);
            }
        }

        public async Task SendEmailConfirmationAsync(string email, string name, string confirmationCode)
        {
            var subject = "Confirm Your SpaceFlow Account";
            var encodedCode = System.Web.HttpUtility.UrlEncode(confirmationCode);
            var confirmationLink = $"https://localhost:7000/api/Account/confirm-email?userId={System.Web.HttpUtility.UrlEncode(email)}&code={encodedCode}";

            var body = $@"
                <html>
                <body>
                    <h2>Welcome to SpaceFlow, {name}!</h2>
                    <p>Thank you for creating an account with us. To complete your registration, please confirm your email address by clicking the link below:</p>
                    <p><a href='{confirmationLink}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Confirm Email Address</a></p>
                    <p>If the button doesn't work, you can copy and paste this link into your browser:</p>
                    <p>{confirmationLink}</p>
                    <p>This link will expire in 24 hours.</p>
                    <br>
                    <p>If you didn't create this account, please ignore this email.</p>
                    <br>
                    <p>Best regards,<br>The SpaceFlow Team</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPasswordResetAsync(string email, string name, string resetCode)
        {
            var subject = "Reset Your SpaceFlow Password";
            var encodedCode = System.Web.HttpUtility.UrlEncode(resetCode);
            var resetLink = $"https://localhost:7000/reset-password?email={System.Web.HttpUtility.UrlEncode(email)}&code={encodedCode}";

            var body = $@"
                <html>
                <body>
                    <h2>Password Reset Request</h2>
                    <p>Hi {name},</p>
                    <p>We received a request to reset your password for your SpaceFlow account. Click the link below to create a new password:</p>
                    <p><a href='{resetLink}' style='background-color: #dc3545; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Reset Password</a></p>
                    <p>If the button doesn't work, you can copy and paste this link into your browser:</p>
                    <p>{resetLink}</p>
                    <p>This link will expire in 1 hour for security reasons.</p>
                    <br>
                    <p>If you didn't request a password reset, please ignore this email or contact us if you have concerns.</p>
                    <br>
                    <p>Best regards,<br>The SpaceFlow Team</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendBookingConfirmationAsync(string email, string name, string workspaceName, DateTime startTime, DateTime endTime)
        {
            var subject = "Booking Confirmation - SpaceFlow";
            var duration = endTime - startTime;
            var durationText = $"{duration.TotalHours:F1} hours";

            var body = $@"
                <html>
                <body>
                    <h2>Booking Confirmed!</h2>
                    <p>Hi {name},</p>
                    <p>Your booking has been confirmed. Here are the details:</p>
                    
                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;'>
                        <h3>Booking Details</h3>
                        <p><strong>Workspace:</strong> {workspaceName}</p>
                        <p><strong>Date:</strong> {startTime:MMMM dd, yyyy}</p>
                        <p><strong>Time:</strong> {startTime:h:mm tt} - {endTime:h:mm tt}</p>
                        <p><strong>Duration:</strong> {durationText}</p>
                    </div>

                    <p>Please arrive on time and bring a valid ID. If you need to make any changes to your booking, please contact us as soon as possible.</p>
                    
                    <p>We look forward to providing you with an excellent workspace experience!</p>
                    <br>
                    <p>Best regards,<br>The SpaceFlow Team</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendBookingCancellationAsync(string email, string name, string workspaceName, DateTime startTime)
        {
            var subject = "Booking Cancellation - SpaceFlow";

            var body = $@"
                <html>
                <body>
                    <h2>Booking Cancelled</h2>
                    <p>Hi {name},</p>
                    <p>Your booking has been cancelled as requested.</p>
                    
                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;'>
                        <h3>Cancelled Booking Details</h3>
                        <p><strong>Workspace:</strong> {workspaceName}</p>
                        <p><strong>Date & Time:</strong> {startTime:MMMM dd, yyyy 'at' h:mm tt}</p>
                    </div>

                    <p>If you paid for this booking, any applicable refunds will be processed according to our refund policy.</p>
                    
                    <p>We hope to serve you again in the future!</p>
                    <br>
                    <p>Best regards,<br>The SpaceFlow Team</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPaymentReceiptAsync(string email, string name, decimal amount, string bookingDetails)
        {
            var subject = "Payment Receipt - SpaceFlow";

            var body = $@"
                <html>
                <body>
                    <h2>Payment Receipt</h2>
                    <p>Hi {name},</p>
                    <p>Thank you for your payment. Here's your receipt:</p>
                    
                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;'>
                        <h3>Payment Details</h3>
                        <p><strong>Amount Paid:</strong> ₹{amount:F2}</p>
                        <p><strong>Date:</strong> {DateTime.Now:MMMM dd, yyyy 'at' h:mm tt}</p>
                        <p><strong>Booking:</strong> {bookingDetails}</p>
                        <p><strong>Status:</strong> <span style='color: green;'>Completed</span></p>
                    </div>

                    <p>This email serves as your official receipt. Please keep it for your records.</p>
                    
                    <p>Thank you for choosing SpaceFlow!</p>
                    <br>
                    <p>Best regards,<br>The SpaceFlow Team</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendWelcomeEmailAsync(string email, string name)
        {
            var subject = "Welcome to SpaceFlow Newsletter!";

            var body = $@"
                <html>
                <body>
                    <h2>Welcome to SpaceFlow, {name}!</h2>
                    <p>Thank you for subscribing to our newsletter. You'll now receive updates about:</p>
                    <ul>
                        <li>New workspace locations</li>
                        <li>Special offers and discounts</li>
                        <li>Productivity tips and workspace trends</li>
                        <li>Exclusive member benefits</li>
                    </ul>
                    <p>We're excited to help you find the perfect workspace for your needs!</p>
                    <br>
                    <p>Best regards,<br>The SpaceFlow Team</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendContactMessageAsync(ContactFormDto contactDto)
        {
            var subject = $"New Contact Form Submission from {contactDto.Name}";

            var body = $@"
                <html>
                <body>
                    <h2>New Contact Form Submission</h2>
                    <p><strong>Name:</strong> {contactDto.Name}</p>
                    <p><strong>Email:</strong> {contactDto.Email}</p>
                    <p><strong>Phone:</strong> {contactDto.PhoneNumber ?? "Not provided"}</p>
                    <p><strong>Subject:</strong> {contactDto.Subject}</p>
                    <br>
                    <p><strong>Message:</strong></p>
                    <div style='background-color: #f8f9fa; padding: 15px; border-left: 4px solid #007bff;'>
                        {contactDto.Message}
                    </div>
                    <br>
                    <p>Please respond to this inquiry promptly.</p>
                </body>
                </html>";

            // Send to admin/support email
            var adminEmail = _configuration["EmailSettings:AdminEmail"] ?? "admin@spaceflow.com";
            await SendEmailAsync(adminEmail, subject, body);

            // Send confirmation to user
            await SendContactConfirmationAsync(contactDto.Email, contactDto.Name);
        }

        private async Task SendContactConfirmationAsync(string email, string name)
        {
            var subject = "Thank you for contacting SpaceFlow";

            var body = $@"
                <html>
                <body>
                    <h2>Thank you for your message!</h2>
                    <p>Hi {name},</p>
                    <p>We've received your message and will get back to you within 24 hours.</p>
                    <p>Our team is committed to providing you with the best possible service.</p>
                    <br>
                    <p>Best regards,<br>The SpaceFlow Team</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }
    }
}