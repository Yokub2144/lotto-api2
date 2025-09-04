using LottoApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// เชื่อมต่อ MySQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);

var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
app.Urls.Add($"http://*:{port}");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Swagger/OpenAPI
}

// Minimal API
app.MapGet("/lotto", () => "Hello Lotto!");
app.MapGet("/User", async (AppDbContext db) =>
{
    return await db.User.ToListAsync();
});

app.Run("http://0.0.0.0:8080");
