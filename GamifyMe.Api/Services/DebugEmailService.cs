namespace GamifyMe.Api.Services
{
    public class DebugEmailService : IEmailService
    {
        private readonly ILogger<DebugEmailService> _logger;

        public DebugEmailService(ILogger<DebugEmailService> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string to, string subject, string body)
        {
            // On simule l'envoi d'email en l'écrivant dans la console de débogage
            _logger.LogWarning("--- NOUVEL EMAIL SIMULÉ ---");
            _logger.LogInformation($"À: {to}");
            _logger.LogInformation($"Sujet: {subject}");
            _logger.LogInformation($"Corps: {body}");
            _logger.LogWarning("--- FIN DE L'EMAIL SIMULÉ ---");

            return Task.CompletedTask;
        }
    }
}