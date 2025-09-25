using WebApp.Models.UserModel;

namespace WebApp.Models.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User> GetByUsername(string userName);
    Task<IEnumerable<UserViewModel>> GetUserOverview();
    Task AddUsers(IEnumerable<User> users);
    Task Delete(IEnumerable<User> users);
}