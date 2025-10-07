using Microsoft.EntityFrameworkCore;
using WebApp.API.Data;
using WebApp.API.Repositories;
using WebApp.API.Service;

//應用程式進入點，配置服務和 Middleware pipeline
var builder = WebApplication.CreateBuilder(args);

// ===== 服務配置 ====

// 註冊控制器服務
builder.Services.AddControllers();

//註冊DB Context，使用SQL Server作為資料庫提供者
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TodoList")));


// ==== Dependecy Injection =====
// 註冊Todo相關服務
builder.Services.AddScoped<ITodoRepository, TodoRepository>();
builder.Services.AddScoped<ITodoService, TodoService>();
// 註冊用戶相關服務
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// ==== 建置應用程式 ====
var app = builder.Build();

// ===== HTTP request pipeline 配置 =====

// 開發環境中啟用詳細錯誤頁面
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
// 強制使用 HTTPS 重定向
app.UseHttpsRedirection();
// 啟用授權Middleware
app.UseAuthorization(); 
// 映射控制器路由
app.MapControllers();

// 啟動應用程式
app.Run();

