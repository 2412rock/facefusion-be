using System.Net.Mail;
using System.Net;

namespace FacefusionBE.Services
{
    public class MailService
    {
        public void SendMailToUser(string verificationCode, string recipientEmail)
        {
            // Sender's Gmail credentials
            string senderEmail = "overflowthegame@gmail.com";
            string senderPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWD");

            // Create a new MailMessage
            MailMessage mail = new MailMessage(senderEmail, recipientEmail)
            {
                Subject = "Verification email",
                Body = $"This is your verification code {verificationCode}"
            };

            // Configure the SMTP client
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true
            };

            try
            {
                // Send the email
                smtpClient.Send(mail);
                Console.WriteLine("Email sent successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email using password {senderPassword}: {ex.Message}");
                throw;
            }
        }
    }
}
