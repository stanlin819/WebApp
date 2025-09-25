using MailKit;
using MailKit.Net.Smtp;
using MimeKit;
using WebApp.Models.UserModel;

namespace WebApp.Models.Service;

public class MailService : IMailService
{
    public async Task SendEmailAsync(List<string> files, string mail, string username)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("BillManager", "acs108142@gm.ntcu.edu.tw"));
        message.To.Add(new MailboxAddress(username, mail));
        message.Cc.Add(new MailboxAddress("Youxuan", "yoyo20614555@gmail.com"));
        message.Subject = "User Report";


        var textPart = new TextPart("plain")
        {
            Text = $"Hi, {username}. Attachments is your user report."
        };
        var multipart = new Multipart("mixed");
        multipart.Add(textPart);

        foreach (var file in files)
        {
            var ext = Path.GetExtension(file);

            var fileBytes = File.ReadAllBytes(file);
            var stream = new MemoryStream(fileBytes);
            var attachment = new MimePart("application", ext.Substring(1))
            {
                //附件內容來源。
                Content = new MimeContent(stream),
                //告訴郵件客戶端這個內容是「附件」，而不是內嵌在信件正文。
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                //決定郵件附件在傳輸過程中的編碼方式。
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = $"{username}_Report{ext}"
            };
            multipart.Add(attachment);

            File.Delete(file);
        }

        message.Body = multipart;

        using (var client = new SmtpClient())
        {
            await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync("acs108142@gm.ntcu.edu.tw", "tmku oarh agvt gavt");
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }


        Console.WriteLine("Mail Send!!!");
    }

}