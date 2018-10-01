using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;

namespace my_app_server.Models
{
    public static class SendEmail
    {
        public static void SendInvitationEmail(Users user, string password)
        {
            var fromAddress = new MailAddress("shatteredplainsgame@gmail.com", "Shattered Plains Game");
            var toAddress = new MailAddress(user.Email, user.Name);
            const string fromPassword = "xPj667jk";
            const string subject = "Account has been created!";
            string body = $"<html><body>" +
                $"<div style=\"font-family: Arial, Helvetica, sans-serif; font-size: 16px; text-align:center;\"> " +
                $"<h2 style=\"color:darkred;\">{user.Name}</h2>" +
                $"<h2>Your account has been successfully created!</h2>" +
                $"<p>You can now enter the game on https://mateuszmichi.github.io." +
                $"<p>Log in with username: <span style=\"color:darkblue; \">{user.Name}</span> and password passed during registry.</p>" +
                $"</div></body></html>";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                Timeout = 20000
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            })
            {
                smtp.Send(message);
            }
        }
        public static void SendContactEmail(string from, string title, string content)
        {
            var fromAddress = new MailAddress(from, from);
            var toAddress = new MailAddress("shatteredplainsgame@gmail.com", "Shattered Plains Game");
            const string fromPassword = "xPj667jk";
            string subject = "[Contact Form] " + title;
            string body = content;
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("shatteredplainsgame@gmail.com", fromPassword),
                Timeout = 20000
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
            })
            {
                smtp.Send(message);
            }
        }
    }
}
