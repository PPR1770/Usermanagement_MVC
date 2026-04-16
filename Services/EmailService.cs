using MailKit.Net.Smtp;
using MimeKit;

namespace ChatApp.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(MailboxAddress.Parse(_config["Email:From"]));
                message.To.Add(MailboxAddress.Parse(to));
                message.Subject = subject;
                message.Body = new TextPart("html") { Text = body };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(
                    _config["Email:Host"],
                    int.Parse(_config["Email:Port"]!),
                    false);
                await smtp.AuthenticateAsync(
                    _config["Email:Username"],
                    _config["Email:Password"]);
                await smtp.SendAsync(message);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Email sent to {To} with subject {Subject}", to, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                throw;
            }
        }
    }
}
