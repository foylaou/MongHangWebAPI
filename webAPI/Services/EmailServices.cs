using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using webAPI.Context;

namespace webAPI.Services;

/// <summary>
/// 表示電子郵件的選項，包括收件人、副本、密件副本等資訊。
/// </summary>
public class EmailOption
{
    /// <summary>
    /// 收件人電子郵件地址。
    /// </summary>
    public string To { get; set; }

    /// <summary>
    /// 郵件主旨。
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// 郵件內容。
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// 副本 (CC) 電子郵件地址列表。
    /// </summary>
    public List<string>? Cc { get; set; } = new List<string>();

    /// <summary>
    /// 密件副本 (BCC) 電子郵件地址列表。
    /// </summary>
    public List<string>? Bcc { get; set; } = new List<string>();
}
public class EmailSettings
{
    /// <summary>
    /// SMTP 伺服器地址。
    /// </summary>
    public string SmtpServer { get; set; } 

    /// <summary>
    /// SMTP 伺服器端口號。
    /// </summary>
    public int SmtpPort { get; set; }

    /// <summary>
    /// SMTP 使用者名稱。
    /// </summary>
    public string SmtpUsername { get; set; }

    /// <summary>
    /// SMTP 密碼。
    /// </summary>
    public string SmtpPassword { get; set; }

    /// <summary>
    /// 發件人電子郵件地址。
    /// </summary>
    public string FromEmail { get; set; }

    /// <summary>
    /// 發件人名稱。
    /// </summary>
    public string FromName { get; set; }

    /// <summary>
    /// 是否啟用 SSL 加密。
    /// </summary>
    public bool EnableSsl { get; set; }
}

public interface IEmailService
{
    Task SendEmailAsync(EmailOption option);
    
}


public class EmailServices:IEmailService

{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailServices> _logger;
    private readonly WebAPIDbcontext _context;
    private readonly EmailSettings _settings;
    public EmailServices(
        IConfiguration configuration,
        ILogger<EmailServices> logger,
        WebAPIDbcontext context,
        IOptions<EmailSettings> settings
        )
    {
    _settings = settings.Value;
    _configuration = configuration;
    _logger=logger;
    _context = context;
    _settings.SmtpServer = _configuration.GetValue<string>("EmailCommectStrings:Smtp_HostName");
    _settings.SmtpPort = _configuration.GetValue<int>("EmailCommectStrings:Port");
    _settings.SmtpUsername = _configuration.GetValue<string>("EmailCommectStrings:From_Mail");
    _settings.SmtpPassword = _configuration.GetValue<string>("EmailCommectStrings:Auth_Code");
    _settings.FromEmail = _configuration.GetValue<string>("EmailCommectStrings:From_Mail");
    _settings.FromName = _configuration.GetValue<string>("EmailCommectStrings:from");
    _settings.EnableSsl = _configuration.GetValue<bool>("EmailCommectStrings:SSL");

    }
    /// <summary>
    /// 電子信箱寄信服務
    /// </summary>
    /// <param name="email"></param>
    /// <param name="subject"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    /// <summary>
    /// 發送電子郵件，支援收件人、副本 (CC) 和密件副本 (BCC)。
    /// </summary>
    /// <param name="option">包含電子郵件發送資訊的選項。</param>
    /// <exception cref="EmailServiceException">當郵件發送失敗時拋出異常。</exception>
    public async Task SendEmailAsync(EmailOption option)
    {
        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = option.Subject,
                Body = option.Body,
                IsBodyHtml = true
            };

            // 添加收件人
            message.To.Add(option.To);

            // 添加 CC
            if (option.Cc != null)
            {
                foreach (var cc in option.Cc)
                {
                    message.CC.Add(cc);
                }
            }

            // 添加 BCC
            if (option.Bcc != null)
            {
                foreach (var bcc in option.Bcc)
                {
                    message.Bcc.Add(bcc);
                }
            }

            using var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword),
                EnableSsl = _settings.EnableSsl
            };

            await client.SendMailAsync(message);
            _logger.LogInformation($"Email sent successfully to {option.To}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send email to {option.To}");
            throw new EmailServiceException("Failed to send email", ex);
        }
    }
    

    /// <summary>
    /// 產生電子郵件 HTML 模板。
    /// </summary>
    /// <param name="title">郵件標題。</param>
    /// <param name="content">郵件內容。</param>
    /// <returns>返回完整的 HTML 格式郵件內容。</returns>
    private string GenerateEmailTemplate(string title, string content)
    {
        string svgContent = System.IO.File.ReadAllText("wwwroot/Images/logo.svg");
        
        string styledSvgContent = svgContent.Replace("<svg ", "<svg style='width: 70%; height: auto;' ");

        return @$"
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset='UTF-8'>
        <title>{title}</title>
    </head>
    <body style='font-family: Arial, sans-serif; line-height: 1.6; max-width: 600px; margin: 0 auto; padding: 20px;'>
        <div style='background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
            <h2 style='color: #333; margin-bottom: 20px;'>{title}</h2>
            <div style='color: #666;'>
                {content}
            </div>
            <hr style='margin: 20px 0; border: none; border-top: 1px solid #eee;'>
            <p style='color: #999; font-size: 12px;'>
                此郵件由系統自動發送，請勿直接回覆。如有問題，請聯繫系統管理員。
            </p>
        </div>
        <div style='text-align: center; margin-top: 20px;'>
            {styledSvgContent}
        </div>
    </body>
    </html>";
    }
    /// <summary>
    /// 寄送驗證碼服務
    /// </summary>
    /// <param name="to"> 給誰</param>
    /// <param name="code">驗證碼</param>
    /// <returns></returns>

    public async Task<bool>SendVerificationEmailCodeAsync(string to, string code)
    {
        var subject = "電子郵件地址驗證碼";
        var body = GenerateEmailTemplate(
            "電子郵件登入驗證",
            @$"
<p style='margin-bottom: 16px;'>親愛的用戶，您好！</p>

<p style='margin-bottom: 16px;'>您正在進行電子郵件登入驗證，請使用以下驗證碼完成登入流程：</p>

<div style='background-color: #f8f9fa; border-left: 4px solid #4285f4; padding: 16px; margin: 20px 0; font-family: monospace; font-size: 24px; text-align: center; letter-spacing: 5px;'>{code}</div>

<p style='margin-bottom: 16px;'>此驗證碼將在 5 分鐘內有效。<br>如果您並未要求進行此操作，請忽略此郵件，並考慮檢查您的帳號安全。</p>

"
        );
    
        var option = new EmailOption
        {
            To = to,
            Subject = subject,
            Body = body,
        };

        await SendEmailAsync(option);
        return true;
    }


}
/// <summary>
/// Email 錯誤拋出
/// </summary>
public class EmailServiceException : Exception
{
    public EmailServiceException(string message) : base(message)
    {
    }

    public EmailServiceException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}