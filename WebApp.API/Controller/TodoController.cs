using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text.Json;


/// <summary>
/// Todo項目的API控制器
/// 提供待辦事項的新增、查詢、刪除和狀態切換功能
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TodoAPIController : ControllerBase
{
    private readonly ITodoService _service;

    /// <summary>
    /// 構造函數
    /// </summary>
    /// <param name="service">Todo服務的依賴注入</param>
    public TodoAPIController(ITodoService service)
    {
        _service = service;
    }

    /// <summary>
    /// 新增一個Todo項目
    /// </summary>
    /// <param name="todo">要新增的Todo項目</param>
    /// <returns>返回新建的Todo項目</returns>
    /// <response code="200">成功建立Todo項目</response>
    [HttpPost("AddTodo")]
    public async Task<IActionResult> AddTodo(Todo todo)
    {
        todo.time = DateTime.Now;
        await _service.AddTodo(todo);
        return Ok(todo);
    }

    /// <summary>
    /// 獲取指定用戶的所有Todo項目
    /// </summary>
    /// <param name="userId">用戶ID</param>
    /// <returns>Todo項目列表</returns>
    /// <response code="200">返回Todo列表</response>
    /// <response code="204">沒有找到Todo項目</response>
    [HttpGet("GetTodos")]
    public async Task<IActionResult> GetTodos(int userId)
    {
        var todos = await _service.GetTodos(userId);
        if (todos == null ||　todos.Count <= 0)
        {
            return NoContent();
        }
        return Ok(todos);
    }

    /// <summary>
    /// 刪除指定的Todo項目
    /// </summary>
    /// <param name="todoId">要刪除的Todo項目ID</param>
    /// <returns>刪除成功返回OK，找不到項目返回NotFound</returns>
    /// <response code="200">成功刪除Todo項目</response>
    /// <response code="404">找不到指定的Todo項目</response>
    [HttpDelete("DeleteTodo/{todoId}")]
    public async Task<IActionResult> DeleteTodo(int todoId)
    {
        var t = await _service.GetTodo(todoId);
        if (t == null)
            return NotFound();
        await _service.DeleteTodo(todoId);
        return Ok();
    }

    /// <summary>
    /// 切換Todo項目的完成狀態
    /// </summary>
    /// <param name="todoId">要切換狀態的Todo項目ID</param>
    /// <returns>返回更新後的Todo項目</returns>
    /// <response code="200">成功切換狀態並返回更新後的Todo項目</response>
    /// <response code="404">找不到指定的Todo項目</response>
    [HttpPatch("Toggle/{todoId}")]
    public async Task<IActionResult> Toggle(int todoId)
    {
        var todo = await _service.Toggle(todoId);
        if (todo != null)
        {
            return Ok(todo);
        }
        return NotFound();
    }
}