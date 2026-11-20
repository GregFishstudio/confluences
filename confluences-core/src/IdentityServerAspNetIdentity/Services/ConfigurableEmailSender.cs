using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace IdentityServerAspNetIdentity.Services
{
    // Serveur SMTP (ex. Infomaniak : mail.infomaniak.com, port 587), dans la section "Smtp" de la
    // configuration du serveur (appsettings.Production.json ou variables d'environnement
    // Smtp__Host, Smtp__User…). Le mot de passe ne va jamais dans Git.
    public class SmtpOptions
    {
        public string Host { get; set; }
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string User { get; set; }
        public string Password { get; set; }
        public string From { get; set; } = "info@confluences.ch";
        public string FromName { get; set; } = "Confluences";
    }

    public class EmailNonConfigureException : InvalidOperationException
    {
        public EmailNonConfigureException()
            : base("Aucun envoi d'e-mail n'est configuré (section Smtp ou clé SendGrid).") { }
    }

    // Envoi des e-mails du serveur de connexion (mot de passe oublié) : SMTP si configuré, sinon
    // SendGrid si une clé est présente, sinon EmailNonConfigureException.
    public class ConfigurableEmailSender : IEmailSender
    {
        private readonly SmtpOptions _smtp;
        private readonly AuthMessageSenderOptions _sendGrid;

        public ConfigurableEmailSender(IOptions<SmtpOptions> smtp, IOptions<AuthMessageSenderOptions> sendGrid)
        {
            _smtp = smtp.Value;
            _sendGrid = sendGrid.Value;
        }

        public bool EstConfigure => !string.IsNullOrWhiteSpace(_smtp.Host) || !string.IsNullOrWhiteSpace(_sendGrid.SendGridKey);

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (!string.IsNullOrWhiteSpace(_smtp.Host))
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(_smtp.From, _smtp.FromName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true,
                };
                message.To.Add(email);
                using var client = new SmtpClient(_smtp.Host, _smtp.Port)
                {
                    EnableSsl = _smtp.EnableSsl,
                    Credentials = string.IsNullOrEmpty(_smtp.User) ? null : new NetworkCredential(_smtp.User, _smtp.Password),
                };
                await client.SendMailAsync(message);
                return;
            }

            if (!string.IsNullOrWhiteSpace(_sendGrid.SendGridKey))
            {
                var client = new SendGridClient(_sendGrid.SendGridKey);
                var msg = new SendGridMessage
                {
                    From = new EmailAddress("info@confluences.ch", _sendGrid.SendGridUser ?? "Confluences"),
                    Subject = subject,
                    HtmlContent = htmlMessage,
                };
                msg.AddTo(new EmailAddress(email));
                msg.SetClickTracking(false, false);
                var response = await client.SendEmailAsync(msg);
                if ((int)response.StatusCode >= 400)
                    throw new InvalidOperationException($"SendGrid a refusé l'envoi ({(int)response.StatusCode}).");
                return;
            }

            throw new EmailNonConfigureException();
        }
    }
}
