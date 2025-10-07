using WebApp.Models.Repositories;
using WebApp.Models.Service;
using WebApp.Data;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using WebApp.Models.Options;
using System.Net.WebSockets;
using System.Text;
using Microsoft.Data.SqlClient;


namespace WebApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BillDB")));

        builder.Services.Configure<FileSettingOptions>(
            builder.Configuration.GetSection("FileUploadSettings")
        );

        builder.Services.Configure<VideoSettingOptions>(
            builder.Configuration.GetSection("VideoUploadSetting")
        );

        builder.Services.Configure<MailSettingOptions>(
            builder.Configuration.GetSection("MailAccountSetting")
        );

        var assembly = Assembly.GetExecutingAssembly();

        // 註冊所有依賴
        foreach (var type in assembly.GetTypes())
        {
            if (type.IsClass && !type.IsAbstract && !type.Name.Equals("SingletonGuidService"))
            {
                var interfaces = type.GetInterfaces()
                                    .Where(i => i.Name == $"I{type.Name}"); // 例如 UserService -> IUserService

                foreach (var iface in interfaces)
                {
                    builder.Services.AddScoped(iface, type);
                }
            }
        }

        builder.Services.AddScoped<ScopedGuidService>();
        builder.Services.AddTransient<TransientGuidService>();
        builder.Services.AddSingleton<SingletonGuidService>();

        builder.Services.AddSingleton<WebSocketService>();
        // Add services to the container.
        builder.Services.AddControllersWithViews();


        var app = builder.Build();
        app.Use(async (context, next) =>
        {
            context.Response.Headers["Cross-Origin-Opener-Policy"] = "same-origin";
            context.Response.Headers["Cross-Origin-Embedder-Policy"] = "require-corp";
            await next();
        });


        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var retries = 5;
            var delay = TimeSpan.FromSeconds(5);

            for (int i = 0; i < retries; i++)
            {
                try
                {
                    db.Database.Migrate();
                    break; // 成功就跳出
                }
                catch (SqlException)
                {
                    Console.WriteLine($"資料庫尚未就緒，等待 {delay.Seconds} 秒後重試...");
                    Thread.Sleep(delay);
                }
            }
        }

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        var webSocketOptions = new WebSocketOptions()
        {
            KeepAliveInterval = TimeSpan.FromSeconds(30), // 心跳間隔
        };

        app.UseWebSockets(webSocketOptions);

        // WebSocket 路由
        app.Map("/ws", async context =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocketService = context.RequestServices.GetRequiredService<WebSocketService>();
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var id = webSocketService.AddSocket(webSocket);

                await webSocketService.ReceiveAsync(id, webSocket);
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        });


        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseStaticFiles();

        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=User}/{action=Index}/{id?}")
            .WithStaticAssets();
    
        app.Run();
    }
}


