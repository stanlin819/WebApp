using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text.Json;

using WebApp.Models.Repositories;
using WebApp.Models.Service;
using WebApp.Models.TodoModel;

[ApiController]
[Route("api/[controller]")]
public class TodoAPIController : ControllerBase
{
    private readonly ITodoService _service;
    public TodoAPIController(ITodoService service)
    {
        _service = service;
    }

    [HttpPost("AddTodo")]
    public async Task<IActionResult> AddTodo([FromForm] Todo todo)
    {
        todo.time = DateTime.Now;
        await _service.AddTodo(todo);
        return Ok(todo);
    }

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

    [HttpDelete("DeleteTodo/{todoId}")]
    public async Task<IActionResult> DeleteTodo(int todoId)
    {
        var t = await _service.GetTodo(todoId);
        if (t == null)
            return NotFound();
        await _service.DeleteTodo(todoId);
        return Ok();
    }

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