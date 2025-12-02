using Microsoft.EntityFrameworkCore;
using Minio;
using Travelink.Inventory.Services;

var builder = WebApplication.CreateBuilder(args);

// Agregamos la conexión a PostgreSQL
builder.Services.AddDbContext<Travelink.Inventory.Data.InventoryContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar MinIO
var minioEndpoint = builder.Configuration["MinIO:Endpoint"];
var minioAccessKey = builder.Configuration["MinIO:AccessKey"];
var minioSecretKey = builder.Configuration["MinIO:SecretKey"];
var minioUseSSL = bool.Parse(builder.Configuration["MinIO:UseSSL"] ?? "false");

builder.Services.AddSingleton<IMinioClient>(sp =>
{
    return new MinioClient()
        .WithEndpoint(minioEndpoint)
        .WithCredentials(minioAccessKey, minioSecretKey)
        .WithSSL(minioUseSSL)
        .Build();
});

builder.Services.AddScoped<IMinioService, MinioService>();

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Ejecutar migraciones automáticamente al iniciar
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Travelink.Inventory.Data.InventoryContext>();
    try
    {
        context.Database.Migrate();
        Console.WriteLine("Migraciones ejecutadas exitosamente");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error ejecutando migraciones: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Health check endpoint para Docker
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
