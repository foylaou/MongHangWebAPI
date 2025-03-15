using Isopoh.Cryptography.Argon2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using webAPI.Context;
using webAPI.Models.資料庫.使用者;

namespace webAPI.Services;



public class RegisterUserDTO
{
    public string Name { get; set; }
    public string NickName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    
}

public interface IAuthServices
{
    Task<(bool sucess, string message)> Register(RegisterUserDTO user);
}

public class AuthServices : IAuthServices
{
    private readonly WebAPIDbcontext _context;
    private readonly ILogger _logger;

    public AuthServices(WebAPIDbcontext context, ILogger<AuthServices> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(bool sucess,string message)> Register(RegisterUserDTO user)
    {
        //檢查信箱格式
        if (string.IsNullOrEmpty(user.Email) || !user.Email.Contains("@") || string.IsNullOrWhiteSpace(user.Email))
        {
            return (false, "信箱格式錯誤或空值");
        }

        if (await _context.Users.AnyAsync(u => u.Email == user.Email))
        {
            return (false, "此Email已使用");
        }

        if (await _context.Users.AnyAsync(u => u.Name == user.Name))
        {
            return (false, "帳號已經被註冊過");
        }
        string hashedpassword = Argon2.Hash(user.Password,
            type:Argon2Type.HybridAddressing,
            timeCost:2,
            memoryCost:32768,
            parallelism: 2);
        var userDB = new User
        {
            Id = Guid.NewGuid(),
            Name = user.Name,
            NickName = user.NickName,
            Email = user.Email,
            Password = hashedpassword,
            UpDateTime = DateTime.Now,
            CreateDateTime = DateTime.Now,
        };
        _context.Users.Add(userDB);
        await _context.SaveChangesAsync();
        
        
        return (true, "註冊成功");
    }
}