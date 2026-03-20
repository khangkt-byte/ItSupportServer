using System.Net;
using System.Net.Mail;

namespace ItSupportServer.src.Shared.Helpers
{
    public static class SendMail
    {
        public static async Task<bool> SendMailAsync(
            IConfiguration config,
            string to,
            string subject,
            string content,
            string? code,
            bool isSendCode = true)
        {
            var fromEmail = config["Email:FromEmail"] ?? string.Empty;
            var appPassword = config["Email:AppPassword"] ?? string.Empty;

            if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(appPassword))
                return false;

            var html = isSendCode && !string.IsNullOrEmpty(code)
                ? BuildOtpTemplate(subject, content, code)
                : BuildLinkTemplate(subject, content);

            try
            {
#pragma warning disable SYSLIB0021 // SmtpClient is obsolete but no built-in async alternative in .NET 10 BCL yet
                using var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromEmail, appPassword)
                };
#pragma warning restore SYSLIB0021

                using var message = new MailMessage(fromEmail, to)
                {
                    Subject = subject,
                    Body = html,
                    IsBodyHtml = true
                };

                await smtpClient.SendMailAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                return false;
            }
        }

        private static string BuildOtpTemplate(string subject, string content, string code) => $@"
<!DOCTYPE html>
<html lang=""vi"">
<head>
    <meta charset=""UTF-8"">
    <title>{subject}</title>
    <style>
        body {{ margin: 0; padding: 0; background-color: #f5f5f5; font-family: Arial, Helvetica, sans-serif; }}
        .email-wrapper {{ width: 100%; padding: 24px 0; background-color: #f5f5f5; }}
        .email-container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 6px rgba(0,0,0,0.1); }}
        .email-header {{ text-align: center; padding: 24px; background: #1565c0; color: #ffffff; }}
        .email-header h1 {{ margin: 8px 0 0 0; font-size: 22px; font-weight: bold; }}
        .email-body {{ padding: 24px; color: #333333; font-size: 14px; line-height: 1.6; text-align: center; }}
        .email-body p {{ margin: 0 0 12px 0; }}
        .email-footer {{ text-align: center; padding: 16px 24px; font-size: 12px; color: #888888; background-color: #fafafa; }}
        .code {{ display: inline-block; margin-top: 16px; padding: 16px 32px; border: 2px solid #1565c0; border-radius: 10px; font-size: 28px; font-weight: bold; letter-spacing: 12px; color: #1565c0; }}
    </style>
</head>
<body>
    <div class=""email-wrapper"">
        <div class=""email-container"">
            <div class=""email-header""><h1>IT Support System</h1></div>
            <div class=""email-body"">
                <p>{content}</p>
                <div class=""code"">{code}</div>
            </div>
            <div class=""email-footer"">Đây là email tự động từ hệ thống IT Support. Vui lòng không trả lời trực tiếp email này.</div>
        </div>
    </div>
</body>
</html>";

        private static string BuildLinkTemplate(string subject, string content) => $@"
<!DOCTYPE html>
<html lang=""vi"">
<head>
    <meta charset=""UTF-8"">
    <title>{subject}</title>
    <style>
        body {{ margin: 0; padding: 0; background-color: #f5f5f5; font-family: Arial, Helvetica, sans-serif; }}
        .email-wrapper {{ width: 100%; padding: 24px 0; background-color: #f5f5f5; }}
        .email-container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 6px rgba(0,0,0,0.1); }}
        .email-header {{ text-align: center; padding: 24px; background: #1565c0; color: #ffffff; }}
        .email-header h1 {{ margin: 8px 0 0 0; font-size: 22px; font-weight: bold; }}
        .email-body {{ padding: 24px; color: #333333; font-size: 14px; line-height: 1.6; text-align: center; }}
        .email-body p {{ margin: 0 0 12px 0; }}
        .email-footer {{ text-align: center; padding: 16px 24px; font-size: 12px; color: #888888; background-color: #fafafa; }}
        .btn-primary {{ display: inline-block; margin-top: 16px; padding: 10px 24px; background-color: #1565c0; color: #ffffff !important; text-decoration: none; border-radius: 4px; font-weight: bold; font-size: 14px; }}
    </style>
</head>
<body>
    <div class=""email-wrapper"">
        <div class=""email-container"">
            <div class=""email-header""><h1>IT Support System</h1></div>
            <div class=""email-body"">
                <p>{content}</p>
            </div>
            <div class=""email-footer"">Đây là email tự động từ hệ thống IT Support. Vui lòng không trả lời trực tiếp email này.</div>
        </div>
    </div>
</body>
</html>";
    }
}
