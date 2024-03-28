using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SneakerStore.Service.Send_Email
{
    public class Email : IEmail
    {
        public MailMessage CreateMailMessage()
        {
            return new MailMessage();
        }

        public SmtpClient CreateSmtpClient()
        {
            return new SmtpClient();
        }
    }
}
