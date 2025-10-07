/// <summary>
/// 待辦事項服務介面，定義待辦事項相關的業務操作契約
/// </summary>
public interface ITodoService
{
    /// <summary>
    /// 取得特定用戶的所有待辦事項
    /// </summary>
    /// <param name="userId">用戶 ID</param>
    /// <returns>該用戶的代辦事項列表</returns>
    Task<List<Todo>> GetTodos(int userId);

    /// <summary>
    /// 根據待辦事項 ID 取得特定待辦事項
    /// </summary>
    /// <param name="id">待辦事項 ID</param>
    /// <returns>找到的待辦事項物件，若不存在則返回 null</returns>
    Task<Todo> GetTodo(int id);

    /// <summary>
    /// 新增代辦事項
    /// </summary>
    /// <param name="todo">要新增的待辦事項物件</param>
    /// <returns>新增後的待辦事項物件</returns>
    Task<Todo> AddTodo(Todo todo);

    /// <summary>
    /// 刪除代辦事項
    /// </summary>
    /// <param name="id">要刪除的待辦事項 ID</param>
    /// <returns></returns>
    Task DeleteTodo(int id);

    /// <summary>
    /// 更新待辦事項資訊
    /// </summary>
    /// <param name="todo">要更新的待辦事項物件</param>
    /// <returns></returns>
    Task UpdateTodo(Todo todo);

    /// <summary>
    /// 切換待辦事項的完成狀態
    /// </summary>
    /// <param name="id">要切換的待辦事項 ID</param>
    /// <returns>更新後的待辦事項物件</returns>
    Task<Todo> Toggle(int id);
}