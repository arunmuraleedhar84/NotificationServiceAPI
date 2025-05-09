using MailKit.Net.Smtp;
using MimeKit;
using NotificationServiceAPI.Models;

namespace NotificationServiceAPI.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config,ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(EmailMessage emailData)
        {
            try { 
            _logger.LogInformation("Attempting to send email to {Email}", emailData.To);
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["Email:From"]));
            email.To.Add(MailboxAddress.Parse(emailData.To));
            email.Subject = emailData.Subject;
            email.Body = new TextPart("plain") { Text = emailData.Body };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_config["Email:SmtpHost"], int.Parse(_config["Email:SmtpPort"]), MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_config["Email:From"], _config["Email:Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
            _logger.LogInformation("Email successfully  to send email to {Email}", emailData.To);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending email to {Email}", emailData.To);

            }
        }

    }
}
