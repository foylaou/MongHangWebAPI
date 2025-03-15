using Microsoft.EntityFrameworkCore;
using webAPI.Context;
using webAPI.Services;

namespace webAPI.Configuration;

public static class ServiceConfiguration
{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        
        var services = builder.Services;

        
        // 註冊 DbContext 服務
        services.AddDbContext<WebAPIDbcontext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("mssql"))
        );



        //帳號管理服務
        services.AddScoped<AuthServices>();
    }
}