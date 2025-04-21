using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace EduQuiz_.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendPasswordSetupEmailAsync(string email, string name, string setupUrl)
        {
            Console.WriteLine($"Attempting to send password setup email to: {email}");
            Console.WriteLine($"Setup URL: {setupUrl}");

            var subject = "Set Up Your EduQuiz Account";
            
            // Plain text version
            var plainTextBody = $@"
Hello {name},

An account has been created for you on EduQuiz. To set up your password and activate your account, please visit the following link:

{setupUrl}

Important: This link will expire in 24 hours for security reasons.

If you did not request this account, please ignore this email.

Best regards,
EduQuiz Team";

            // HTML version
            var htmlBody = $@"
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .button {{ display: inline-block; background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #777; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Welcome to EduQuiz!</h2>
                        </div>
                        <div class='content'>
                            <p>Hello {name},</p>
                            <p>An account has been created for you on EduQuiz. To set up your password and activate your account, please click the button below:</p>
                            
                            <div style='text-align: center;'>
                                <a href='{setupUrl}' class='button'>Set Up Your Password</a>
                            </div>
                            
                            <p>If the button doesn't work, you can copy and paste this link into your browser:</p>
                            <p style='word-break: break-all;'>{setupUrl}</p>
                            
                            <p><strong>Important:</strong> This link will expire in 24 hours for security reasons.</p>
                            <p>If you did not request this account, please ignore this email.</p>
                        </div>
                        <div class='footer'>
                            <p>Best regards,<br />EduQuiz Team</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(email, subject, htmlBody, plainTextBody);
        }

        public async Task SendPasswordResetEmailAsync(string email, string name, string resetToken)
        {
            var baseUrl = _configuration["ApplicationUrl"] ?? "http://localhost:5000";
            var resetUrl = $"{baseUrl}/Account/ResetPassword?token={resetToken}&email={Uri.EscapeDataString(email)}";

            var subject = "Reset Your EduQuiz Password";
            
            // Plain text version
            var plainTextBody = $@"
Hello {name},

We received a request to reset your password. To reset your password, please visit the following link:

{resetUrl}

If you did not request a password reset, please ignore this email.
This link will expire in 1 hour for security reasons.

Best regards,
EduQuiz Team";
            
            // HTML version
            var htmlBody = $@"
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .button {{ display: inline-block; background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #777; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Password Reset Request</h2>
                        </div>
                        <div class='content'>
                            <p>Hello {name},</p>
                            <p>We received a request to reset your password. To reset your password, please click the button below:</p>
                            
                            <div style='text-align: center;'>
                                <a href='{resetUrl}' class='button'>Reset Your Password</a>
                            </div>
                            
                            <p>If the button doesn't work, you can copy and paste this link into your browser:</p>
                            <p style='word-break: break-all;'>{resetUrl}</p>
                            
                            <p>If you did not request a password reset, please ignore this email.</p>
                            <p>This link will expire in 1 hour for security reasons.</p>
                        </div>
                        <div class='footer'>
                            <p>Best regards,<br />EduQuiz Team</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(email, subject, htmlBody, plainTextBody);
        }

        private async Task SendEmailAsync(string to, string subject, string htmlBody, string plainTextBody)
        {
            try
            {
                Console.WriteLine($"Starting email send process to: {to}");
                Console.WriteLine("Reading SMTP settings from configuration...");
                
                var smtpSettings = _configuration.GetSection("SmtpSettings").Get<SmtpSettings>();
                if (smtpSettings == null)
                {
                    throw new InvalidOperationException("SMTP settings not configured");
                }

                Console.WriteLine($"SMTP Server: {smtpSettings.Server}");
                Console.WriteLine($"SMTP Port: {smtpSettings.Port}");
                Console.WriteLine($"SMTP Username: {smtpSettings.Username}");
                Console.WriteLine($"From Email: {smtpSettings.FromEmail}");

                // Validate SMTP settings
                if (string.IsNullOrEmpty(smtpSettings.Server))
                    throw new InvalidOperationException("SMTP server is not configured");
                
                if (smtpSettings.Port <= 0)
                    throw new InvalidOperationException("SMTP port is not configured correctly");
                
                if (string.IsNullOrEmpty(smtpSettings.Username) || string.IsNullOrEmpty(smtpSettings.Password))
                    throw new InvalidOperationException("SMTP credentials are not configured");
                
                if (string.IsNullOrEmpty(smtpSettings.FromEmail))
                    throw new InvalidOperationException("SMTP sender email is not configured");

                Console.WriteLine("Configuring SMTP client...");
                using (var client = new SmtpClient())
                {
                    client.Host = smtpSettings.Server;
                    client.Port = smtpSettings.Port;
                    client.EnableSsl = true;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password);
                    client.Timeout = 30000; // 30 seconds
                    
                    // Allow self-signed certificates and handle SSL/TLS properly
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = 
                        (sender, certificate, chain, sslPolicyErrors) => true;

                    Console.WriteLine("Creating email message...");
                    using (var message = new MailMessage())
                    {
                        // Use a friendly name for the sender
                        string fromName = !string.IsNullOrEmpty(smtpSettings.FromName) 
                            ? smtpSettings.FromName 
                            : "EduQuiz Team";
                        
                        message.From = new MailAddress(smtpSettings.FromEmail, fromName);
                        message.To.Add(new MailAddress(to));
                        message.Subject = subject;
                        message.Body = htmlBody;
                        message.IsBodyHtml = true;
                        
                        // Add plain text alternative view
                        var plainTextView = AlternateView.CreateAlternateViewFromString(
                            plainTextBody, null, "text/plain");
                        var htmlView = AlternateView.CreateAlternateViewFromString(
                            htmlBody, null, "text/html");
                        
                        message.AlternateViews.Add(plainTextView);
                        message.AlternateViews.Add(htmlView);

                        // Add headers to improve deliverability
                        message.Headers.Add("X-Priority", "1");
                        message.Headers.Add("X-MSMail-Priority", "High");
                        message.Headers.Add("Importance", "High");
                        message.Headers.Add("X-Mailer", "Microsoft ASP.NET Core");

                        Console.WriteLine("Sending email...");
                        await client.SendMailAsync(message);
                        Console.WriteLine("Email sent successfully!");
                    }
                }
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"SMTP Error: {ex.Message}");
                Console.WriteLine($"Status Code: {ex.StatusCode}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }
    }

    public class SmtpSettings
    {
        public required string Server { get; set; }
        public required int Port { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string FromEmail { get; set; }
        public string? FromName { get; set; }
    }
} 