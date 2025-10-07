using Microsoft.AspNetCore.Mvc;
using WebApp.API.Service;


[ApiController]
[Route("api/[controller]")]
public class UserAPIController : ControllerBase
{
    private readonly IUserService _service;

    public UserAPIController(IUserService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var allUser = await _service.GetAllUsers();
        return Ok(allUser);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var user = await _service.GetUser(id);
        if (user != null)
            return Ok(user);
        else
            return NotFound();
    }

    [HttpPost("AddUser")]
    public async Task<IActionResult> AddUser(User user)
    {
        await _service.AddUser(user);
        return Ok(user);
    }

    [HttpDelete("DeleteUser/{userId}")]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        var u = await _service.GetUser(userId);
        if (u == null)
            return NotFound(new { message = $"User with ID {userId} not found" });
        var mes = await _service.DeleteUser(userId);
        return Ok(new { message = mes });

    }
    
    [HttpPut("UpdateUser")]
    public async Task<IActionResult> UpdateUser(User user)
    {
        // 更新用戶
        var result = await _service.UpdateUser(user);
        
        // 檢查更新結果
        if (result.Contains("not found"))
        {
            return NotFound(new { message = result });
        }
        else if (result.Contains("already exists"))
        {
            return BadRequest(new { message = result });
        }
        
        return Ok(new { message = result });
    }
}