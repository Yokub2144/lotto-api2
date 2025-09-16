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

// เพิ่ม Controllers ก่อน Build
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://*:{port}");


app.MapOpenApi();
app.UseSwagger();            // สร้าง JSON สำหรับ Swagger
app.UseSwaggerUI();   // Swagger/OpenAPI

// Minimal API
app.MapGet("/", () => "Hello from Lotto API!");
app.MapGet("/lotto", () => "Hello Lotto!");
app.MapGet("/User", async (AppDbContext db) =>
{
    return await db.User.ToListAsync();
});

app.MapGet("/admin",()=>"aSDasdasd");

// Map controller endpoints (ถ้าใช้ controller)
app.MapControllers();

app.Run();