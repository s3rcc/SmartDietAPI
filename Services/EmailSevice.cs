using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Services.Interfaces;
using System.Threading.Tasks;

namespace Services
{
    public class EmailSevice : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailSevice(IConfiguration configuration)
        {
            _configuration = configuration;
        }



        public async Task SendEmailAsync(string sendTo, string subject, string body, string templateType)
        {
            #region template
            var accountVerificationTemplate = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f9f9f9;
            border-radius: 5px;
        }}
        .header {{
            background-color: #4CAF50;
            color: white;
            padding: 10px;
            text-align: center;
            border-radius: 5px 5px 0 0;
        }}
        .content {{
            padding: 20px;
            background-color: white;
            border-radius: 0 0 5px 5px;
        }}
        .code-block {{
            background-color: #f0f0f0;
            padding: 15px;
            border-radius: 5px;
            font-family: 'Courier New', monospace;
            font-size: 16px;
            text-align: center;
        }}
        .footer {{
            margin-top: 20px;
            font-size: 12px;
            color: #666;
            text-align: center;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2><strong>Smart Diet</strong> - Xác minh tài khoản của bạn</h2>
        </div>
        <div class='content'>
            <p>Xin chào,</p>
            <p>Vui lòng xác minh tài khoản <strong>Smart Diet</strong> của bạn bằng mã dưới đây:</p>
            <div class='code-block'>{body}</div>
            <p>Mã này sẽ hết hạn trong 10 phút. Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này hoặc liên hệ với bộ phận hỗ trợ.</p>
            <p>Chào mừng bạn đến với <strong>Smart Diet</strong>!<br>Đội ngũ <strong>Smart Diet</strong></p>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} <strong>Smart Diet</strong>. Mọi quyền được bảo lưu.</p>
            <p>Hỗ trợ: phutg2000@gmail.com</p>
        </div>
    </div>
</body>
</html>";

            var resetPasswordTemplate = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f9f9f9;
            border-radius: 5px;
        }}
        .header {{
            background-color: #2196F3;
            color: white;
            padding: 10px;
            text-align: center;
            border-radius: 5px 5px 0 0;
        }}
        .content {{
            padding: 20px;
            background-color: white;
            border-radius: 0 0 5px 5px;
        }}
        .code-block {{
            background-color: #f0f0f0;
            padding: 15px;
            border-radius: 5px;
            font-family: 'Courier New', monospace;
            font-size: 18px;
            text-align: center;
            letter-spacing: 2px;
        }}
        .footer {{
            margin-top: 20px;
            font-size: 12px;
            color: #666;
            text-align: center;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2><strong>Smart Diet</strong> - Đặt lại mật khẩu của bạn</h2>
        </div>
        <div class='content'>
            <p>Xin chào,</p>
            <p>Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu cho tài khoản <strong>Smart Diet</strong> của bạn. Sử dụng mã dưới đây để tiếp tục:</p>
            <div class='code-block'>{body}</div>
            <p>Mã này có hiệu lực trong 10 phút. Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng liên hệ với chúng tôi ngay lập tức.</p>
            <p>Trân trọng,<br>Đội ngũ <strong>Smart Diet</strong></p>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} <strong>Smart Diet</strong>. Mọi quyền được bảo lưu.</p>
            <p>Hỗ trợ: phutg2000@gmail.com</p>
        </div>
    </div>
</body>
</html>";

            var newUserNotificationTemplate = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f9f9f9;
            border-radius: 5px;
        }}
        .header {{
            background-color: #673AB7;
            color: white;
            padding: 10px;
            text-align: center;
            border-radius: 5px 5px 0 0;
        }}
        .content {{
            padding: 20px;
            background-color: white;
            border-radius: 0 0 5px 5px;
        }}
        .credentials {{
            background-color: #f0f0f0;
            padding: 15px;
            border-radius: 5px;
            margin: 10px 0;
        }}
        .credentials p {{
            margin: 5px 0;
        }}
        .button {{
            display: inline-block;
            padding: 12px 24px;
            background-color: #673AB7;
            color: white;
            text-decoration: none;
            border-radius: 5px;
            margin: 10px 0;
        }}
        .footer {{
            margin-top: 20px;
            font-size: 12px;
            color: #666;
            text-align: center;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Chào mừng bạn đến với <strong>Smart Diet</strong>!</h2>
        </div>
        <div class='content'>
            <p>Xin chào,</p>
            <p>Một tài khoản mới đã được tạo cho bạn trong hệ thống <strong>Smart Diet</strong>. Dưới đây là thông tin đăng nhập của bạn:</p>
            <div class='credentials'>
                <p><strong>Email:</strong> {sendTo}</p>
                <p><strong>Mật khẩu tạm thời:</strong> {body}</p>
            </div>
            <p>Vui lòng sử dụng thông tin này để đăng nhập và đổi mật khẩu ngay khi có thể:</p>
            <a href='https://smartdiet.com/login' class='button'>Đăng nhập ngay</a>
            <p>Nếu bạn có bất kỳ câu hỏi nào, đừng ngần ngại liên hệ với đội ngũ hỗ trợ của chúng tôi.</p>
            <p>Trân trọng,<br>Đội ngũ <strong>Smart Diet</strong></p>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} <strong>Smart Diet</strong>. Mọi quyền được bảo lưu.</p>
            <p>Hỗ trợ: phutg2000@gmail.com</p>
        </div>
    </div>
</body>
</html>";
            #endregion

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Smart Diet Team", _configuration["MailSettings:Mail"]));
            email.To.Add(new MailboxAddress("", sendTo));
            email.Subject = subject;

            string htmlBody = templateType switch
            {
                "ResetPassword" => resetPasswordTemplate,
                "AccountVerification" => accountVerificationTemplate,
                "NewStaff" => newUserNotificationTemplate,
                _ => throw new ArgumentException("Invalid template type")
            };

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody.Replace("{body}", body) };
            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                await smtp.ConnectAsync(
                    _configuration["MailSettings:Host"],
                    int.Parse(_configuration["MailSettings:Port"]!),
                    MailKit.Security.SecureSocketOptions.StartTls);

                await smtp.AuthenticateAsync(
                    _configuration["MailSettings:Mail"],
                    _configuration["MailSettings:Password"]);

                await smtp.SendAsync(email);
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }

        
    }
}