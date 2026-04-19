using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FleetManager.WebMVC.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SmtpEmailSender> _logger;
        public SmtpEmailSender(IConfiguration config, ILogger<SmtpEmailSender> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var from = _config["Smtp:From"] ?? "no-reply@example.com";
            var host = _config["Smtp:Host"] ?? "localhost";
            var port = int.TryParse(_config["Smtp:Port"], out var p) ? p : 25;
            var user = _config["Smtp:User"];
            var pass = _config["Smtp:Pass"];
            var enableSsl = bool.TryParse(_config["Smtp:EnableSsl"], out var ssl) && ssl;

            var msg = new MailMessage(from, to, subject, body);
            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Timeout = 20000
            };

            if (!string.IsNullOrEmpty(user))
            {
                client.Credentials = new NetworkCredential(user, pass);
            }

            try
            {
                await client.SendMailAsync(msg);
                _logger?.LogInformation("SMTP: Email sent to {To}", to);
            }
            catch (SmtpException ex)
            {
                _logger?.LogError(ex, "SMTP failed to send email to {To}: {Message}", to, ex.Message);
                throw;
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "SMTP unexpected error when sending email to {To}: {Message}", to, ex.Message);
                throw;
            }
        }
    }
}
