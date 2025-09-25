using Microsoft.AspNetCore.Mvc;
using WebApp.Models.Service;


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
}