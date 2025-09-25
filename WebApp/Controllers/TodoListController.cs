using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text.Json;

using WebApp.Models.Repositories;
using WebApp.Models.Service;
using WebApp.Models.TodoModel;

public class TodoListController : Controller
{
    private readonly ITodoService _service;
    public TodoListController(ITodoService service)
    {
        _service = service;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> AddTodo([FromForm] Todo t)
    {
        await _service.AddTodo(t);
        return Json(new { success = true, Todo = t });
    }

    public async Task<IActionResult> GetTodos(int userId)
    {
        var todos = await _service.GetTodos(userId);
        return Json(todos);
    }

    public async Task<IActionResult> DeleteTodo(int todoId)
    {
        await _service.DeleteTodo(todoId);
        return Json(new { success = true });
    }

    public async Task<IActionResult> Toggle(int todoId)
    {
        var todo = await _service.GetTodo(todoId);
        if (todo != null)
        {
            todo.done = (todo.done) ? false : true;
            await _service.UpdateTodo(todo);
            return Json(new { success = true });
        }
        return Json(new { success = false });
    }

    public IActionResult TodoListView()
    {
        return PartialView("_TodoList");
    }
}