using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;

namespace GamifyMe.Api.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtpSettings = _configuration.GetSection("Smtp");
            var host = smtpSettings["Host"];
            var port = int.Parse(smtpSettings["Port"] ?? "465");
            var senderEmail = smtpSettings["SenderEmail"];
            var password = smtpSettings["Password"];

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("MeritoPass", senderEmail));
                message.To.Add(new MailboxAddress("", to));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = body };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                // Utilisation de 10 secondes de timeout pour chaque étape
                client.Timeout = 10000; 

                // Connexion avec Auto (STARTTLS souvent sur 587, SSL sur 465)
                await client.ConnectAsync(host, port, SecureSocketOptions.Auto);

                // Authentification
                await client.AuthenticateAsync(senderEmail, password);

                // Envoi
                await client.SendAsync(message);
                _logger.LogInformation($"Email sent to {to}");

                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email to {to}");
                throw;
            }
        }
    }
}
