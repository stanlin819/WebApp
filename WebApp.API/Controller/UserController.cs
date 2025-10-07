using Microsoft.AspNetCore.Mvc;
using WebApp.API.Service;


[ApiController]
[Route("api/[controller]")]
public class UserAPIController : ControllerBase
{
    private readonly UserService _service;

    public UserAPIController(UserService service)
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
}