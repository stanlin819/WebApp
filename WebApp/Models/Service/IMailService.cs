using WebApp.Models.UserModel;

namespace WebApp.Models.Service;

/// <summary>
/// 郵件服務介面
/// 定義系統郵件發送相關功能
/// </summary>
public interface IMailService
{
    /// <summary>
    /// 非同步發送郵件
    /// </summary>
    /// <param name="files">要附加的檔案路徑列表</param>
    /// <param name="mail">收件者信箱</param>
    /// <param name="username">收件者名稱</param>
    /// <returns>發送完成的Task</returns>
    Task SendEmailAsync(List<string> files, string mail, string username);

}