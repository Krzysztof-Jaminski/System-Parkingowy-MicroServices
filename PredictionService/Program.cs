using Microsoft.EntityFrameworkCore;
using PredictionService.Models;
using PredictionService.RabbitMQ;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// Dodaj DbContext (SQL Server lokalny)
builder.Services.AddDbContext<PredictionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Start RabbitMQ consumer (in background)
Task.Run(() => {
    var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
    var consumer = new RabbitMqConsumer(scopeFactory);
    consumer.StartConsuming();
});

app.MapControllers();

app.Run();

public partial class Program { }
