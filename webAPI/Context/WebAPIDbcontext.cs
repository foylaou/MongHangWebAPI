using Isopoh.Cryptography.Argon2;
using Microsoft.EntityFrameworkCore;
using webAPI.Models.資料庫.使用者;

namespace webAPI.Context;

public partial class WebAPIDbcontext(DbContextOptions<WebAPIDbcontext> options,IConfiguration configuration)
: DbContext(options)
{
    /// <summary>
    /// 建立資料表
    /// </summary>
    public DbSet<User> Users { get; set; }
    
    /// <summary>
    /// 連接字串
    /// </summary>
    /// <param name="optionsBuilder"></param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {

            var connectionString = configuration.GetConnectionString("mssql");
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
    /// <summary>
    /// 用來做關連
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        SeedData(modelBuilder);

    }
    /// <summary>
    /// 初始化資料
    /// </summary>
    /// <param name="modelBuilder"></param>
    private void SeedData(ModelBuilder modelBuilder)
    {

        const string password = "t0955787053S";
        var hashPassword = Argon2.Hash(password,
            type: Argon2Type.HybridAddressing, // 使用 Argon2id（最安全）
            timeCost:2, // 計算次數（1~4 之間，越大越安全但慢）
            memoryCost:32768, // 記憶體使用（32MB，64MB 可能太高）
            parallelism:2 // 並行執行的 CPU 線程（1~4，設 2 讓多核 CPU 負擔平均）
            );
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                NickName = "系統管理者",
                Password = hashPassword,
                Email = "s225002731@gmail.com",
                CreateDateTime = DateTime.Now,
                UpDateTime = DateTime.Now,
            }
        );

    }

}
