using Microsoft.EntityFrameworkCore;
using UserService.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dodaj DbContext (SQLite na start)
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

// Start RabbitMQ consumer (in background)
Task.Run(() => {
    var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
    var consumer = new UserService.RabbitMQ.RabbitMqConsumer(scopeFactory);
    consumer.StartConsuming();
});

app.Run();

public partial class Program { }
