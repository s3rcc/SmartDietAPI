using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Org.BouncyCastle.Utilities;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class EmailSevice : IEmailService
    {
        private readonly IConfiguration _configuration;
        public EmailSevice(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task SendEmailAsync(string sendTo, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Smart Diet", _configuration["MailSettings:Mail"]));
            email.To.Add(new MailboxAddress("Member", sendTo));
            email.Subject = subject;
            var bodybuilder = new BodyBuilder()
            {
                HtmlBody = body
            };
            email.Body = bodybuilder.ToMessageBody();
            var smtp = new SmtpClient();
            try
            {
                await smtp.ConnectAsync(
                    _configuration["MailSettings:Host"],
                    int.Parse( _configuration["MailSettings:Port"]!),
                    MailKit.Security.SecureSocketOptions.StartTls);

                await smtp.AuthenticateAsync(
                    _configuration["MailSettings:Mail"], 
                    _configuration["MailSettings:Password"]);
                
                await smtp.SendAsync(email);
            }
            finally
            {
                await smtp.DisconnectAsync(true);
                 smtp.Dispose();
            }
        }
    }
}
