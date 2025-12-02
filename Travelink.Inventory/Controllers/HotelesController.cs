using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Travelink.Inventory.Data;
using Travelink.Inventory.Models;
using Travelink.Inventory.Services;

[Route("api/[controller]")]
[ApiController]
public class HotelesController : ControllerBase
{
    private readonly InventoryContext _context;
    private readonly IMinioService _minioService;

    public HotelesController(InventoryContext context, IMinioService minioService)
    {
        _context = context;
        _minioService = minioService;
    }

    // GET: api/Hoteles
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Hotel>>> GetHoteles()
    {
        return await _context.Hoteles.ToListAsync();
    }

    // GET: api/Hoteles/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Hotel>> GetHotel(int id)
    {
        var hotel = await _context.Hoteles.FindAsync(id);

        if (hotel == null)
        {
            return NotFound();
        }

        return hotel;
    }

    // POST: api/Hoteles
    [HttpPost]
    public async Task<ActionResult<Hotel>> PostHotel([FromForm] HotelCreateDto hotelDto)
    {
        // Subir imágenes a MinIO
        var imageUrls = new List<string>();
        if (hotelDto.ImagenesArchivos != null && hotelDto.ImagenesArchivos.Any())
        {
            imageUrls = await _minioService.UploadMultipleImagesAsync(hotelDto.ImagenesArchivos);
        }

        var hotel = new Hotel
        {
            Nombre = hotelDto.Nombre,
            Ciudad = hotelDto.Ciudad,
            Direccion = hotelDto.Direccion,
            Activo = hotelDto.Activo,
            Imagenes = imageUrls
        };

        _context.Hoteles.Add(hotel);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetHotel), new { id = hotel.Id }, hotel);
    }

    // PUT: api/Hoteles/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateHotel(int id, [FromForm] HotelUpdateDto hotelDto)
    {
        var hotel = await _context.Hoteles.FindAsync(id);

        if (hotel == null)
        {
            return NotFound();
        }

        hotel.Nombre = hotelDto.Nombre;
        hotel.Ciudad = hotelDto.Ciudad;
        hotel.Direccion = hotelDto.Direccion;
        hotel.Activo = hotelDto.Activo;

        // Si hay nuevas imágenes, subirlas
        if (hotelDto.ImagenesArchivos != null && hotelDto.ImagenesArchivos.Any())
        {
            var newImageUrls = await _minioService.UploadMultipleImagesAsync(hotelDto.ImagenesArchivos);
            hotel.Imagenes.AddRange(newImageUrls);
        }

        _context.Entry(hotel).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Hoteles/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHotel(int id)
    {
        var hotel = await _context.Hoteles.FindAsync(id);

        if (hotel == null)
        {
            return NotFound();
        }

        // Eliminar imágenes de MinIO
        foreach (var imageUrl in hotel.Imagenes)
        {
            await _minioService.DeleteImageAsync(imageUrl);
        }

        _context.Hoteles.Remove(hotel);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Hoteles/5/imagen
    [HttpDelete("{id}/imagen")]
    public async Task<IActionResult> DeleteHotelImage(int id, [FromBody] string imageUrl)
    {
        var hotel = await _context.Hoteles.FindAsync(id);

        if (hotel == null)
        {
            return NotFound();
        }

        if (hotel.Imagenes.Contains(imageUrl))
        {
            await _minioService.DeleteImageAsync(imageUrl);
            hotel.Imagenes.Remove(imageUrl);
            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Imagen eliminada exitosamente" });
        }

        return NotFound(new { mensaje = "Imagen no encontrada" });
    }
}

// DTOs para el controlador
public class HotelCreateDto
{
    public string Nombre { get; set; }
    public string Ciudad { get; set; }
    public string Direccion { get; set; } // iframe de Google Maps
    public bool Activo { get; set; } = true;
    public List<IFormFile>? ImagenesArchivos { get; set; }
}

public class HotelUpdateDto
{
    public string Nombre { get; set; }
    public string Ciudad { get; set; }
    public string Direccion { get; set; }
    public bool Activo { get; set; }
    public List<IFormFile>? ImagenesArchivos { get; set; }
}