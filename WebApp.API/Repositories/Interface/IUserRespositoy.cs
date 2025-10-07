/// <summary>
/// 用戶資料庫操作介面，定義用戶相關的CRUD操作契約
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// 取得所有用戶資料
    /// </summary>
    /// <returns>包含所有用戶的集合</returns>
    Task<IEnumerable<User>> GetAll();
    /// <summary>
    /// 根據用戶ID取得特定用戶
    /// </summary>
    /// <param name="id">用戶 ID</param>
    /// <returns>找到的用戶物件，若不存在則返回 null</returns>
    Task<User> Get(int id);
    /// <summary>
    /// 根據用戶名取得特定用戶
    /// </summary>
    /// <param name="userName">用戶名稱</param>
    /// <returns>找到的用戶物件，若不存在則返回null</returns>
    Task<User> GetByUserName(string userName);
    /// <summary>
    /// 新增單一用戶
    /// </summary>
    /// <param name="user">要新增的用戶物件</param>
    /// <returns>新增後的用戶物件</returns>
    Task<User> Add(User user);
    /// <summary>
    /// 批量新增多個用戶
    /// </summary>
    /// <param name="users">要新增的用戶集合</param>
    /// <returns>新增的用戶集合</returns>
    Task<IEnumerable<User>> AddUsers(IEnumerable<User> users);
    /// <summary>
    /// 更新現有用戶資訊
    /// </summary>
    /// <param name="user">要更新的用戶物件</param>
    /// <returns>更新後的用戶物件</returns>
    Task<User> Update(User user);
    /// <summary>
    /// 刪除單一用戶
    /// </summary>
    /// <param name="user">要刪除的用戶物件</param>
    /// <returns>被刪除用戶的用戶名稱</returns>
    Task<string> Delete(User user);
    /// <summary>
    /// 批量刪除多個用戶
    /// </summary>
    /// <param name="users">要刪除的用戶集合</param>
    Task Delete(IEnumerable<User> users);
}