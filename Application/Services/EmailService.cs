using System.Net;
using System.Net.Mail;

using Microsoft.Extensions.Configuration;

namespace Application.Services;

public class EmailService(IConfiguration configuration)
{
    public void SendVerify(string to, string code)
    {
        var smtp = new SmtpClient(configuration["SmtpSettings:Host"]);
        smtp.Port = int.Parse(configuration["SmtpSettings:Port"]!);
        smtp.Credentials = new NetworkCredential(configuration["SmtpSettings:Username"], configuration["SmtpSettings:Password"]);
        smtp.EnableSsl = false;

        var mail = new MailMessage();
        mail.From = new MailAddress("Test@example.com");
        mail.To.Add(to);
        mail.Subject = "Email verify";
        mail.Body = "Code: " + code;
        mail.IsBodyHtml = true;

        smtp.SendMailAsync(mail);
    }
}
