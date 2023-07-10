﻿namespace OJS.Services.Common.Emails
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using System.Text;
    using System.Threading.Tasks;

    using Kendo.Mvc.Extensions;

    public class EmailSenderService : IDisposable, IEmailSenderService
    {
        private readonly SmtpClient mailClient;
        private readonly string senderEmail;
        private readonly string senderDisplayName;

        public EmailSenderService(
            string emailServerHost,
            int emailServerPort,
            string username,
            string password,
            string senderEmail,
            string senderDisplayName,
            string devEmail = null)
        {

            this.mailClient = new SmtpClient
            {
                UseDefaultCredentials = false,

            };
            mailClient.Credentials = new NetworkCredential(username, password);
            mailClient.Port = emailServerPort;
            mailClient.Host = emailServerHost;
            mailClient.EnableSsl = true;

            this.senderEmail = senderEmail;
            this.senderDisplayName = senderDisplayName;
            this.DevEmail = devEmail;
        }

        public string DevEmail { get; private set; }

        public void SendEmail(
            string recipient,
            string subject,
            string body,
            IEnumerable<string> bccRecipients = null,
            AttachmentCollection attachments = null)
        {
            var message = this.PrepareMessage(recipient, subject, body, bccRecipients, attachments);
            this.mailClient.Send(message);
        }

        public async Task SendEmailAsync(
            string recipient,
            string subject,
            string body,
            IEnumerable<string> bccRecipients = null)
        {
            var message = this.PrepareMessage(recipient, subject, body, bccRecipients, null);
            await this.mailClient.SendMailAsync(message);
        }

        public void Dispose()
        {
            this.mailClient?.Dispose();
        }

        private MailMessage PrepareMessage(
            string recipient,
            string subject,
            string body,
            IEnumerable<string> bccRecipients,
            AttachmentCollection attachments)
        {
            var mailTo = new MailAddress(recipient);
            var mailFrom = new MailAddress(this.senderEmail, this.senderDisplayName);

            var message = new MailMessage(mailFrom, mailTo)
            {
                Body = body,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true,
                Subject = subject,
                SubjectEncoding = Encoding.UTF8,
            };

            if (bccRecipients != null)
            {
                foreach (var bccRecipient in bccRecipients)
                {
                    message.Bcc.Add(bccRecipient);
                }
            }

            if (attachments != null && attachments.Any())
            {
                message.Attachments.AddRange(attachments);
            }

            return message;
        }

        public string GetDevEmail() => this.DevEmail;
    }
}