using System.Net;
using System.Net.Mail;
using EcommerceStore.Models;
using Microsoft.Extensions.Options;

namespace EcommerceStore.Services
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int Port { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendOrderConfirmationEmailAsync(Order order)
        {
            var subject = $"Order Confirmation - {order.UniqueOrderId}";
            
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>
                        <div style='text-align: center; margin-bottom: 20px;'>
                            <h1 style='color: #0d6efd;'>Order Placed!</h1>
                            <p style='font-size: 1.2em;'>Thank you for your order with Delhi Kitchen.</p>
                        </div>
                        
                        <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin-bottom: 20px; text-align: center;'>
                            <p style='margin: 0; color: #6c757d;'>Your unique Order ID is:</p>
                            <h2 style='margin: 5px 0; color: #0d6efd;'>{order.UniqueOrderId}</h2>
                        </div>

                        <div style='margin-bottom: 20px;'>
                            <p><strong>Delivering to:</strong></p>
                            <p style='margin: 5px 0;'>{order.DeliveryAddress}</p>
                            <p style='margin: 5px 0; color: #6c757d; font-size: 0.9em;'>Contact: {order.PhoneNumber}</p>
                        </div>

                        <p style='font-style: italic; color: #6c757d; font-size: 0.9em;'>
                            You can manage, edit, or cancel your order within the next 12 hours using this ID on our website.
                        </p>

                        <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee;'>
                            <p style='margin: 0; font-weight: bold;'>Delhi Kitchen</p>
                            <p style='margin: 0; font-size: 0.8em; color: #999;'>Pure Vegetarian • Healthy • Fresh</p>
                        </div>
                    </div>
                </body>
                </html>";

            using var client = new SmtpClient(_settings.SmtpServer, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(order.CustomerEmail);

            await client.SendMailAsync(mailMessage);
        }
    }
}
