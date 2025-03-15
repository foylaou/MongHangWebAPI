using System.ComponentModel.DataAnnotations;

namespace webAPI.Models.資料庫.使用者;

public class User
{
    public Guid Id { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    public string NickName { get; set; }
    
    public string Password { get; set; }
    
    public string Email { get; set; }
    
    public DateTime UpDateTime { get; set; }=DateTime.Now;
    
    public DateTime CreateDateTime { get; set; }
    
}