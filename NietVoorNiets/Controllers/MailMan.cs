using System.Collections.Generic;
using System.Net.Mail;

namespace NietVoorNiets.Controllers
{
    public class MailMan
    {
        public void Send(string subject, string body, string receiver)
        {
            Send(subject, body, new string[] { receiver });
        }

        public void Send(string subject, string body, IEnumerable<string> receivers)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("dannybrouwertest@hotmail.com");
            mail.Subject = subject;
            mail.Body = body;

            foreach (var receiver in receivers)
            {
                mail.To.Add(receiver);
            }

            SmtpClient client = new SmtpClient();
            client.Port = 25;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Host = "localhost";
            client.Send(mail);
        }
    }
}