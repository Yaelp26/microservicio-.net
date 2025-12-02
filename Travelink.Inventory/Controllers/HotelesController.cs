using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Travelink.Inventory.Data;
using Travelink.Inventory.Models;

[Route("api/[controller]")]
[ApiController]
public class HotelesController : ControllerBase
{
    private readonly InventoryContext _context;

    public HotelesController(InventoryContext context)
    {
        _context = context;
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
            return NotFound(new { mensaje = "Hotel no encontrado" });
        }

        return hotel;
    }

    // GET: api/Hoteles/buscar/{nombre}
    [HttpGet("buscar/{nombre}")]
    public async Task<ActionResult<IEnumerable<Hotel>>> BuscarHotelPorNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return BadRequest(new { mensaje = "El nombre de búsqueda no puede estar vacío" });
        }

        var hoteles = await _context.Hoteles
            .Where(h => h.Nombre.Contains(nombre))
            .ToListAsync();

        if (!hoteles.Any())
        {
            return NotFound(new { mensaje = "No se encontraron hoteles con ese nombre" });
        }

        return hoteles;
    }

    // POST: api/Hoteles
    [HttpPost]
    public async Task<ActionResult<Hotel>> PostHotel(Hotel hotel)
    {
        // Validar y normalizar estado
        if (!string.IsNullOrEmpty(hotel.EstadoHotel))
        {
            hotel.EstadoHotel = hotel.EstadoHotel.ToLower();
            var estadosValidos = new[] { "disponible", "fueradeservicio" };
            if (!estadosValidos.Contains(hotel.EstadoHotel))
            {
                return BadRequest(new { mensaje = "EstadoHotel debe ser: disponible o fueradeservicio" });
            }
        }

        _context.Hoteles.Add(hotel);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetHotel), new { id = hotel.Id }, hotel);
    }

    // PUT: api/Hoteles/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutHotel(int id, Hotel hotel)
    {
        if (id != hotel.Id)
        {
            return BadRequest(new { mensaje = "El ID no coincide" });
        }

        // Validar y normalizar estado
        if (!string.IsNullOrEmpty(hotel.EstadoHotel))
        {
            hotel.EstadoHotel = hotel.EstadoHotel.ToLower();
            var estadosValidos = new[] { "disponible", "fueradeservicio" };
            if (!estadosValidos.Contains(hotel.EstadoHotel))
            {
                return BadRequest(new { mensaje = "EstadoHotel debe ser: disponible o fueradeservicio" });
            }
        }

        _context.Entry(hotel).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await HotelExists(id))
            {
                return NotFound(new { mensaje = "Hotel no encontrado" });
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/Hoteles/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHotel(int id)
    {
        var hotel = await _context.Hoteles.FindAsync(id);

        if (hotel == null)
        {
            return NotFound(new { mensaje = "Hotel no encontrado" });
        }

        // Verificar si tiene habitaciones asociadas
        var tieneHabitaciones = await _context.Habitaciones.AnyAsync(h => h.HotelId == id);
        if (tieneHabitaciones)
        {
            return BadRequest(new { mensaje = "No se puede eliminar el hotel porque tiene habitaciones asociadas" });
        }

        _context.Hoteles.Remove(hotel);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Hotel eliminado exitosamente" });
    }

    private async Task<bool> HotelExists(int id)
    {
        return await _context.Hoteles.AnyAsync(e => e.Id == id);
    }
}