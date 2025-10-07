using WebApp.API.Repositories;
/// <summary>
/// Todo服務實現類
/// 處理Todo項目的業務邏輯
/// </summary>
public class TodoService : ITodoService
{
    private readonly ITodoRepository _repo;

    /// <summary>
    /// 構造函數
    /// </summary>
    /// <param name="repo">Todo儲存庫的依賴注入</param>
    public TodoService(ITodoRepository repo)
    {
        _repo = repo;
    }

    /// <summary>
    /// 獲取指定用戶的所有Todo項目
    /// </summary>
    /// <param name="userId">用戶ID</param>
    /// <returns>該用戶的Todo項目列表</returns>
    public async Task<List<Todo>> GetTodos(int userId)
    {
        var todos = await _repo.GetUserTodo(userId);
        return todos;
    }

    /// <summary>
    /// 根據ID獲取單個Todo項目
    /// </summary>
    /// <param name="todoId">Todo項目ID</param>
    /// <returns>指定ID的Todo項目，如果不存在則返回null</returns>
    public async Task<Todo> GetTodo(int todoId)
    {
        return await _repo.Get(todoId);
    }

    /// <summary>
    /// 新增Todo項目
    /// </summary>
    /// <param name="t">要新增的Todo項目</param>
    /// <returns>新增後的Todo項目（包含資料庫生成的ID）</returns>
    public async Task<Todo> AddTodo(Todo t)
    {
        return await _repo.Add(t);
    }

    /// <summary>
    /// 刪除指定的Todo項目
    /// </summary>
    /// <param name="id">要刪除的Todo項目ID</param>
    /// <returns>無返回值</returns>
    public async Task DeleteTodo(int id)
    {
        var t = await _repo.Get(id);
        await _repo.Delete(t);
    }

    /// <summary>
    /// 更新Todo項目
    /// </summary>
    /// <param name="todo">要更新的Todo項目</param>
    /// <returns>無返回值</returns>
    public async Task UpdateTodo(Todo todo)
    {
        await _repo.Update(todo);
    }

    /// <summary>
    /// 切換Todo項目的完成狀態
    /// </summary>
    /// <param name="todoId">要切換狀態的Todo項目ID</param>
    /// <returns>更新後的Todo項目，如果項目不存在則返回null</returns>
    public async Task<Todo> Toggle(int todoId)
    {
        var todo = await _repo.Get(todoId);
        if (todo == null)
            return null;
        todo.done = !todo.done;
        await _repo.Update(todo);
        return todo;
    }
}