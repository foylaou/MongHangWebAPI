using Microsoft.AspNetCore.Mvc;
using webAPI.Context;
using webAPI.Services;


namespace webAPI.Controllers;

[Route("User/")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly WebAPIDbcontext _dbcontext;
    private readonly AuthServices _authServices;

    public UserController(
        WebAPIDbcontext dbcontext,
        ILogger<UserController> logger,
        AuthServices authServices

    )
    {
        _logger = logger;
        _dbcontext = dbcontext;
        _authServices = authServices;
    }

    [HttpGet("GetUser")]
    public IActionResult GetUser()
    {
        var user = _dbcontext.Users.ToArray();
        if (true)
        {
            _logger.LogInformation("我查了GetUser");
        }

        if (true)
        {
            _logger.LogError("發生錯誤GetUser");
        }
        if (true)
        {
            _logger.LogCritical("發生錯誤GetUser");
        }
        if (true)
        {
            _logger.LogWarning("發生錯誤GetUser");
        }

        
        return Ok(new { success = true,message ="查詢成功", data = user });
    }

    [HttpPost("Register")]
    public IActionResult Register([FromBody] RegisterUserDTO user)
    {
        var result = _authServices.Register(user).Result;
        if (result.sucess)
        {
            return Ok(new { success = true, message = result.message });
        }

        return BadRequest(new { success = false, message = result.message });
        
    }
    
}