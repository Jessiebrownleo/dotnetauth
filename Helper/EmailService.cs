using MailKit.Net.Smtp;
using MimeKit;

namespace DotnetAuthentication.Helper;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            Console.WriteLine($"Starting email send to {toEmail}");
            Console.WriteLine($"Config: Host={_config["Email:SmtpHost"]}, Port={_config["Email:SmtpPort"]}, User={_config["Email:SmtpUsername"]}");

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_config["Email:SenderName"], _config["Email:SenderEmail"]));
            email.To.Add(new MailboxAddress("", toEmail));
            email.Subject = subject;
            email.Body = new TextPart("html") { Text = body };

            using var smtp = new SmtpClient();
            smtp.MessageSent += (sender, args) => Console.WriteLine("Email sent: " + args.Response);
            smtp.ServerCertificateValidationCallback = (s, c, h, e) => true; 
            Console.WriteLine("Connecting to SMTP server...");
            await smtp.ConnectAsync(_config["Email:SmtpHost"], int.Parse(_config["Email:SmtpPort"]), MailKit.Security.SecureSocketOptions.StartTls);
            Console.WriteLine("Connected. Authenticating...");
            await smtp.AuthenticateAsync(_config["Email:SmtpUsername"], _config["Email:SmtpPassword"]);
            Console.WriteLine("Authenticated. Sending email...");
            await smtp.SendAsync(email);
            Console.WriteLine("Email sent successfully!");
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Email send failed: {ex.Message}\n{ex.StackTrace}");
        }
    }
}