using System.Security.Cryptography;
using System.Text.Json;
using Isopoh.Cryptography.Argon2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using StackExchange.Redis;
using webAPI.Context;
using webAPI.Models.資料庫.使用者;

namespace webAPI.Services;


/// <summary>
/// Redis 存儲驗證碼的資料格式
/// </summary>
public class VerificationData
{
    public string Code { get; set; }
    public int Attempts { get; set; }
}

/// <summary>
/// 註冊用Dto
/// 
/// </summary>
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
    private readonly EmailServices _emailServices;
    private readonly IDatabase _redisDb; // 🔹 加入 Redis 連線

    public AuthServices(WebAPIDbcontext context, ILogger<AuthServices> logger, EmailServices emailServices,IConnectionMultiplexer redis)
    {
        _context = context;
        _logger = logger;
        _emailServices = emailServices;
        _redisDb =redis.GetDatabase(); 
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
    
    
    /// <summary>
    /// 產生 8 碼驗證碼，存入 Redis，並發送 Email
    /// </summary>
    public async Task<bool> SendVerificationCodeAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
            throw new ArgumentException("Email 不可為空", nameof(email));

        // 產生 8 碼驗證碼
        var verificationCode = Generate8DigitNumber();
        var redisKey = $"email_verification:{email}";

        // 儲存驗證碼，設定 5 分鐘過期，並初始化錯誤次數
        var data = new VerificationData { Code = verificationCode, Attempts = 0 };
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        await _redisDb.StringSetAsync(redisKey, JsonSerializer.Serialize(data, options), TimeSpan.FromMinutes(5));
    
        bool emailSent = await _emailServices.SendVerificationEmailCodeAsync(email, verificationCode);
        if (!emailSent)
        {
            _logger.LogError("無法發送驗證碼至 {Email}", email);
            return false;
        }

        _logger.LogInformation("已發送驗證碼至 {Email}: {Code}", email, verificationCode);
        return true;
    }

    public async Task<(bool success, string message)> SendEmailCode(string email)
    {
        
        
        return (true, "發送成功");
    }
    
    
    
    
    static string Generate8DigitNumber()
    {
        // 產生一個隨機的 8 碼數字
        using (var rng = RandomNumberGenerator.Create())
        {
            byte[] bytes = new byte[4]; // 4 bytes 可表示一個 32-bit 整數
            rng.GetBytes(bytes);
            int number = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 100000000; // 限制在 8 位數
            return number.ToString("D8"); // 確保補滿 8 碼
        }
    }
    
}