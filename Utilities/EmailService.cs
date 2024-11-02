using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace PostService.Utilities
{
    public class EmailService
    {
        private readonly string _smtpServer = "sandbox.smtp.mailtrap.io";
        private readonly int _port = 2525;
        private readonly string _username = "d02be295936406"; // Your Mailtrap username
        private readonly string _password = "d12dfb410dd9c1"; // Your Mailtrap password

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var fromAddress = new MailAddress("hasanolp1@gmail.com", "Hasan Hamed");
            var toAddress = new MailAddress(toEmail);

            using (var smtpClient = new SmtpClient(_smtpServer, _port)
            {
                Credentials = new NetworkCredential(_username, _password),
                EnableSsl = true
            })
            using (var mailMessage = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true // Set to true if you want to send HTML emails
            })
            {
                await smtpClient.SendMailAsync(mailMessage);
            }
        }


    }
}
