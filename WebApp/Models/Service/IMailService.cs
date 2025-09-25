using WebApp.Models.UserModel;

namespace WebApp.Models.Service;


public interface IMailService
{
    Task SendEmailAsync(List<string> files, string mail, string username);

}