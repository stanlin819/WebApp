public interface IUserService
{
    Task<IEnumerable<User>> GetAllUsers();
    Task<User> GetUser(int id);
    Task<User> GetUserByUsername(string userName);
    Task<User> AddUser(User user);
    Task<string> AddUsers(IEnumerable<User> users);
    Task<string> UpdateUser(User user);
    Task<string> DeleteUser(int id);
}