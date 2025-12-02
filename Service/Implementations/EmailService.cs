using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Service.Interfaces;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Service.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly string _mailHost;
        private readonly int _mailPort;
        private readonly string _mailUser;
        private readonly string _mailPass;
        private readonly bool _mailEnableSsl;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _logger = logger;

            _mailHost = string.IsNullOrWhiteSpace(config["Email:SmtpHost"])
                ? "smtp.gmail.com"
                : config["Email:SmtpHost"]!;

            if (string.IsNullOrWhiteSpace(_mailHost))
                throw new InvalidOperationException("Email:SmtpHost is missing or empty in configuration.");

            var portString = config["Email:SmtpPort"];
            if (!int.TryParse(portString, out _mailPort))
                _mailPort = 587;

            _mailUser = config["Email:User"] ?? string.Empty;
            _mailPass = config["Email:Password"] ?? string.Empty;
            _mailEnableSsl = bool.TryParse(config["Email:EnableSsl"], out var enableSsl) && enableSsl;

            if (string.IsNullOrWhiteSpace(_mailUser))
                _logger.LogWarning("Email:User is empty — outgoing mail may fail.");
        }
        
        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            try
            {
                using var client = await CreateClientAsync();

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("EV Driver Dev Team", _mailUser));
                message.To.Add(new MailboxAddress(to, to));
                message.Subject = subject;

                var builder = new BodyBuilder
                {
                    HtmlBody = htmlBody
                };
                message.Body = builder.ToMessageBody();

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                throw;
            }
        }
        
        
        private async Task<SmtpClient> CreateClientAsync()
        {
            var client = new SmtpClient();
            var socketOption = _mailEnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;

            await client.ConnectAsync(_mailHost, _mailPort, socketOption);
            await client.AuthenticateAsync(_mailUser, _mailPass);

            return client;
        }
    }
}