/// <summary>
/// 用戶服務介面，定義用戶相關的業務操作契約
/// </summary>
public interface IUserService
{
    /// <summary>
    /// 取得所有用戶及其相關資料
    /// </summary>
    /// <returns>包含所有用戶的集合</returns>
    Task<IEnumerable<User>> GetAllUsers();

    /// <summary>
    /// 根據用戶 ID 取得特定用戶
    /// </summary>
    /// <param name="id">用戶 ID</param>
    /// <returns>找到的用戶物件，若不存在則返回 null</returns>
    Task<User> GetUser(int id);
    
    /// <summary>
    /// 根據用戶名稱取得特定用戶
    /// </summary>
    /// <param name="userName">用戶名稱</param>
    /// <returns>找到的用戶物件，若不存在則返回 null</returns>
    Task<User> GetUserByUsername(string userName);

    /// <summary>
    /// 新增單一用戶
    /// </summary>
    /// <param name="user">要新增的用戶物件</param>
    /// <returns>新增成功返回用戶物件，用戶名重複則返回 null</returns>
    Task<User> AddUser(User user);

    /// <summary>
    /// 批量新增多個用戶
    /// </summary>
    /// <param name="users">要新增的用戶集合</param>
    /// <returns>重複的用戶名稱列表，若無重複則返回空字串</returns>
    Task<string> AddUsers(IEnumerable<User> users);

    /// <summary>
    /// 更新現有用戶資訊
    /// </summary>
    /// <param name="user">要更新的用戶物件</param>
    /// <returns>更新結果訊息</returns>
    Task<string> UpdateUser(User user);

    /// <summary>
    /// 刪除單一用戶
    /// </summary>
    /// <param name="id">要刪除的用戶 ID</param>
    /// <returns>刪除結果訊息</returns>
    Task<string> DeleteUser(int id);

    /// <summary>
    /// 批量刪除多個用戶
    /// </summary>
    /// <param name="users">要刪除的用戶集合</param>
    /// <returns>刪除結果訊息</returns>
    Task<string> DeleteUsers(IEnumerable<User> users);

}