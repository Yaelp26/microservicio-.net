using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Configurar Npgsql para soportar JSON din�mico
var dataSourceBuilder = new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("DefaultConnection"));
dataSourceBuilder.EnableDynamicJson();
var dataSource = dataSourceBuilder.Build();

// Agregamos la conexi�n a PostgreSQL
builder.Services.AddDbContext<Travelink.Inventory.Data.InventoryContext>(options =>
    options.UseNpgsql(dataSource));

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
