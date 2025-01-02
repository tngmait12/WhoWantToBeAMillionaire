using System.Net.Mail;
using System.Net;

namespace WhoWantToBeAMillionaire.Areas.Admin.Data
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true, //bật bảo mật
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("tongmaitruongvu11@gmail.com", "arhayaomivewqgzf")
            };

            return client.SendMailAsync(
                new MailMessage(from: "tongmaitruongvu11@gmail.com",
                                to: email,
                                subject,
                                message
                                )

                );
        }
    }
}
