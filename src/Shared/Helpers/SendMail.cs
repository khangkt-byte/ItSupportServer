using System.Net;
using System.Net.Mail;

namespace ItSupportServer.src.Shared.Helpers
{
    public class SendMail
    {
        public async static Task<bool> SendMailAsync(IConfiguration con, string to, string subject, string Content, string? code, bool IsSendCode = true)
        {
            string fromEmail = con.GetValue<string>("Email:FromEmail"); // Email của bạn
            string appPassword = con.GetValue<string>("Email:AppPassword");       // App password của Gmail
            var html = "";
            if (IsSendCode && !string.IsNullOrEmpty(code))
            {
                html = new SendMail().SendEmailTemplate(new SendCodeTemplate
                {
                    Subject = subject,
                    Content = Content,
                    Code = code
                });
            }
            else
            {
                html = new SendMail().SendEmailTemplate(new SendMailTemplate
                {
                    Subject = subject,
                    Content = Content
                });
            }
            try
            {
                using (var smtpClient = new SmtpClient("smtp.gmail.com"))
                {
                    smtpClient.Port = 587;
                    smtpClient.EnableSsl = true;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(fromEmail, appPassword);

                    using (var message = new MailMessage(fromEmail, to))
                    {
                        message.Subject = subject;
                        message.Body = html; // Nội dung HTML
                        message.IsBodyHtml = true;  // ⭐ Quan trọng: báo cho MailMessage biết đây là HTML

                        await smtpClient.SendMailAsync(message);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
                return false;
            }
        }

        private string SendEmailTemplate(SendCodeTemplate tem)
        {
            return $@"
<!DOCTYPE html>
<html lang=""vi"">
<head>
    <meta charset=""UTF-8"">
    <title>{tem.Subject}</title>
    <style>
        body {{
            margin: 0;
            padding: 0;
            background-color: #f5f5f5;
            font-family: Arial, Helvetica, sans-serif;
        }}
        .email-wrapper {{
            width: 100%;
            padding: 24px 0;
            background-color: #f5f5f5;
        }}
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 2px 6px rgba(0,0,0,0.1);
        }}
        .email-header {{
            text-align: center;
            padding: 24px;
            background: #ff7043;
            color: #ffffff;
        }}
        .email-header img {{
            max-width: 120px;
            height: auto;
            display: block;
            margin: 0 auto 8px auto;
        }}
        .email-header h1 {{
            margin: 8px 0 0 0;
            font-size: 22px;
            font-weight: bold;
        }}
        .email-body {{
            padding: 24px;
            color: #333333;
            font-size: 14px;
            line-height: 1.6;
            text-align: center;
            text-align: center;
        }}
        .email-body p {{
            margin: 0 0 12px 0;
        }}
        .email-footer {{
            text-align: center;
            padding: 16px 24px 24px 24px;
            font-size: 12px;
            color: #888888;
            background-color: #fafafa;
        }}
        .btn-primary {{
            display: inline-block;
            margin-top: 16px;
            padding: 10px 20px;
            background-color: #ff7043;
            color: #ffffff !important;
            text-decoration: none;
            border-radius: 4px;
            font-weight: bold;
            font-size: 14px;
        }}
        .code {{
            padding: 20px;
            border: 2px solid #d7d7d7;
            border-radius: 10px;
            letter-spacing: 15px;
        }}
    </style>
</head>
<body>
    <div class=""email-wrapper"">
        <div class=""email-container"">
            <div class=""email-header"">
                <img src=""https://raw.githubusercontent.com/kienkent1/RestaurantImg/main/logo.png.png"" 
                     alt=""Food Order Logo"" />
                <h1>Food Order xin chào</h1>
            </div>
            <div class=""email-body"">
                {tem.Content}
            <h1 class=""code""><b>{tem.Code}</b></h1>
            </div>
            <div class=""email-footer"">
                Đây là email tự động từ hệ thống Food Order, vui lòng không trả lời trực tiếp email này.
            </div>
        </div>
    </div>
</body>
</html>";
        }

        private string SendEmailTemplate(SendMailTemplate tem)
        {
            return $@"
<!DOCTYPE html>
<html lang=""vi"">
<head>
    <meta charset=""UTF-8"">
    <title>{tem.Subject}</title>
    <style>
        body {{
            margin: 0;
            padding: 0;
            background-color: #f5f5f5;
            font-family: Arial, Helvetica, sans-serif;
        }}
        .email-wrapper {{
            width: 100%;
            padding: 24px 0;
            background-color: #f5f5f5;
        }}
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 2px 6px rgba(0,0,0,0.1);
        }}
        .email-header {{
            text-align: center;
            padding: 24px;
            background: #ff7043;
            color: #ffffff;
        }}
        .email-header img {{
            max-width: 120px;
            height: auto;
            display: block;
            margin: 0 auto 8px auto;
        }}
        .email-header h1 {{
            margin: 8px 0 0 0;
            font-size: 22px;
            font-weight: bold;
        }}
        .email-body {{
            padding: 24px;
            color: #333333;
            font-size: 14px;
            line-height: 1.6;
            text-align: center;
            text-align: center;
        }}
        .email-body p {{
            margin: 0 0 12px 0;
        }}
        .email-footer {{
            text-align: center;
            padding: 16px 24px 24px 24px;
            font-size: 12px;
            color: #888888;
            background-color: #fafafa;
        }}
        .btn-primary {{
            display: inline-block;
            margin-top: 16px;
            padding: 10px 20px;
            background-color: #ff7043;
            color: #ffffff !important;
            text-decoration: none;
            border-radius: 4px;
            font-weight: bold;
            font-size: 14px;
        }}
        .code {{
            padding: 20px;
            border: 2px solid #d7d7d7;
            border-radius: 10px;
            letter-spacing: 15px;
        }}
    </style>
</head>
<body>
    <div class=""email-wrapper"">
        <div class=""email-container"">
            <div class=""email-header"">
                <img src=""https://raw.githubusercontent.com/kienkent1/RestaurantImg/main/logo.png.png"" 
                     alt=""Food Order Logo"" />
                <h1>Food Order xin chào</h1>
            </div>
            <div class=""email-body"">
                {tem.Content}
            </div>
            <div class=""email-footer"">
                Đây là email tự động từ hệ thống Food Order, vui lòng không trả lời trực tiếp email này.
            </div>
        </div>
    </div>
</body>
</html>";
        }
        public class SendCodeTemplate : SendMailTemplate
        {
            public string Code { get; set; }
        }

        public class SendMailTemplate
        {
            public string Subject { get; set; }
            public string Content { get; set; }
        }
    }
}
