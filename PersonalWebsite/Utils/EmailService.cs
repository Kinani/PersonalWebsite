using MailKit.Net.Pop3;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using PersonalWebsite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalWebsite.Utils
{
    public interface IEmailService
    {
        void Send(EmailMessage emailMessage);
        List<EmailMessage> ReceiveEmail(int maxCount = 10);
    }

    public class EmailService : IEmailService
    {
        private IConfiguration _configuration;

        public EmailService(IConfiguration Configuration)
        {
            _configuration = Configuration;
        }

        public List<EmailMessage> ReceiveEmail(int maxCount = 10)
        {
            using (var emailClient = new Pop3Client())
            {
                int port = int.Parse(_configuration.GetSection("EmailConfiguration:PopPort").Value);
                emailClient.Connect(_configuration.GetSection("EmailConfiguration:PopServer").Value,  port, true);

                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

                emailClient.Authenticate(_configuration.GetSection("EmailConfiguration:PopUsername").Value, _configuration.GetSection("EmailConfiguration:PopPassword").Value);

                List<EmailMessage> emails = new List<EmailMessage>();
                for (int i = 0; i < emailClient.Count && i < maxCount; i++)
                {
                    var message = emailClient.GetMessage(i);
                    var emailMessage = new EmailMessage
                    {
                        Content = !string.IsNullOrEmpty(message.HtmlBody) ? message.HtmlBody : message.TextBody,
                        Subject = message.Subject
                    };
                    emailMessage.ToAddresses.AddRange(message.To.Select(x => (MailboxAddress)x).Select(x => new EmailAddress { Address = x.Address, Name = x.Name }));
                    emailMessage.FromAddresses.AddRange(message.From.Select(x => (MailboxAddress)x).Select(x => new EmailAddress { Address = x.Address, Name = x.Name }));
                }

                return emails;
            }
        }

        public void Send(EmailMessage emailMessage)
        {
            var message = new MimeMessage();
            message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
            message.From.AddRange(emailMessage.FromAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));

            message.Subject = emailMessage.Subject;
            //We will say we are sending HTML. But there are options for plaintext etc. 
            message.Body = new TextPart(TextFormat.Html)
            {
                Text = emailMessage.Content
            };

            //Be careful that the SmtpClient class is the one from Mailkit not the framework!
            using (var emailClient = new SmtpClient())
            {
                int port = int.Parse(_configuration.GetSection("EmailConfiguration:SmtpPort").Value);

                //The last parameter here is to use SSL (Which you should!)
                emailClient.Connect(_configuration.GetSection("EmailConfiguration:SmtpServer").Value, port, SecureSocketOptions.Auto);

                //Remove any OAuth functionality as we won't be using it. 
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

                emailClient.Authenticate(_configuration.GetSection("EmailConfiguration:SmtpUsername").Value, _configuration.GetSection("EmailConfiguration:SmtpPassword").Value);

                emailClient.Send(message);

                emailClient.Disconnect(true);
            }
        }
    }
}
