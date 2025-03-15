using webAPI.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
//用controller 控制API
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
//統一服務管理 Configuration
builder.ConfigureServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//用controller 控制API
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
