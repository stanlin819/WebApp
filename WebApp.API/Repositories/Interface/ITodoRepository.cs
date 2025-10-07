namespace WebApp.API.Repositories;

/// <summary>
/// 待辦事項資料庫操作介面，定義待辦事項相關的 CRUD 操作
/// </summary>
public interface ITodoRepository
{
    /// <summary>
    /// 新增代辦事項
    /// </summary>
    /// <param name="todo">要新增的代辦事項物件</param>
    /// <returns>新增後的待辦事項物件</returns>
    Task<Todo> Add(Todo todo);

    /// <summary>
    /// 刪除代辦事項
    /// </summary>
    /// <param name="todo">要刪除的待辦事項物件</param>
    /// <returns></returns>
    Task Delete(Todo todo);

    /// <summary>
    /// 根據待辦事項 ID 取得特定待辦事項
    /// </summary>
    /// <param name="id">待辦事項 ID</param>
    /// <returns>找到的待辦事項物件，若不存在則返回 null</returns>
    Task<Todo> Get(int id);

    /// <summary>
    /// 取得特定用戶的所有待辦事項 
    /// </summary>
    /// <param name="userId">用戶 ID</param>
    /// <returns>該用戶的待辦事項列表</returns>
    Task<List<Todo>> GetUserTodo(int userId);

    /// <summary>
    /// 更新代辦事項資訊
    /// </summary>
    /// <param name="todo">要更新的待辦事項物件</param>
    /// <returns></returns>
    Task Update(Todo todo);
}