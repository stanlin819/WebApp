using Microsoft.EntityFrameworkCore;
using WebApp.API.Data;
using WebApp.API.Repositories;
using WebApp.API.Service;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();  
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TodoList")));

builder.Services.AddScoped<ITodoRepository, TodoRepository>();
builder.Services.AddScoped<ITodoService, TodoService>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthorization(); 
app.MapControllers();


app.Run();

