using MailKit;
using MailKit.Net.Smtp;
using MimeKit;
using WebApp.Models.Options;
using WebApp.Models.UserModel;
using Microsoft.Extensions.Options;
namespace WebApp.Models.Service;

/// <summary>
/// Gmail SMTP 郵件服務實現類
/// </summary>
public class MailService : IMailService
{
    private string account; // Gmail 帳號
    private string password; //Gmail 應用程式密碼

    /// <summary>
    /// 建構函數，注入 Gmail 帳號設定
    /// </summary>
    public MailService(IOptions<MailSettingOptions> options)
    {
        account = options.Value.Mail_account;
        password = options.Value.Mail_password;
    }

    public async Task SendEmailAsync(List<string> files, string mail, string username)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("BillManager", account));
        message.To.Add(new MailboxAddress(username, mail));
        message.Cc.Add(new MailboxAddress("Youxuan", "yoyo20614555@gmail.com"));
        message.Subject = "User Report";

        //建立郵件內文
        var textPart = new TextPart("plain")
        {
            Text = $"Hi, {username}. Attachments is your user report."
        };

        //建立多部分郵件內容
        var multipart = new Multipart("mixed");
        multipart.Add(textPart);

        //處理附件
        foreach (var file in files)
        {
            var ext = Path.GetExtension(file);

            //讀取檔案內容
            var fileBytes = File.ReadAllBytes(file);
            var stream = new MemoryStream(fileBytes);

            //建立附件
            var attachment = new MimePart("application", ext.Substring(1))
            {
                //附件內容來源。
                Content = new MimeContent(stream),
                //告訴郵件客戶端這個內容是「附件」，而不是內嵌在信件正文。
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                //決定郵件附件在傳輸過程中的編碼方式。
                ContentTransferEncoding = ContentEncoding.Base64,
                //附件檔名
                FileName = $"{username}_Report{ext}"
            };
            multipart.Add(attachment);

            //刪除暫存檔案
            File.Delete(file);
        }

        message.Body = multipart;

        // 使用 SMTP 客戶端發送郵件
        using (var client = new SmtpClient())
        {
            // 連接到 Gmail SMTP 伺服器
            await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            // 驗證帳號密碼
            await client.AuthenticateAsync(account, password);
            // 發送郵件
            await client.SendAsync(message);
            // 中斷連線
            await client.DisconnectAsync(true);
        }


        Console.WriteLine("Mail Send!!!");
    }

}