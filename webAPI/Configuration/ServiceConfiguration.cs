using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using webAPI.Context;
using webAPI.Services;

namespace webAPI.Configuration;

public static class ServiceConfiguration
{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        
        var services = builder.Services;
        var configuration = builder.Configuration;
        
        // 註冊 DbContext 服務
        services.AddDbContext<WebAPIDbcontext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("mssql"))
        );
        
        //註冊Redis服務
        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var redisConnectionString = Environment.GetEnvironmentVariable("Redis")
                                        ?? configuration.GetConnectionString("Redis");

            return ConnectionMultiplexer.Connect(redisConnectionString);
        });


        //帳號管理服務
        services.AddScoped<AuthServices>();
    }
}