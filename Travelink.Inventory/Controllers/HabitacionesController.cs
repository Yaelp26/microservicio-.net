using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Travelink.Inventory.Data;
using Travelink.Inventory.Models;

namespace Travelink.Inventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HabitacionesController : ControllerBase
    {
        private readonly InventoryContext _context;

        public HabitacionesController(InventoryContext context)
        {
            _context = context;
        }

        // GET: api/Habitaciones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Habitacion>>> GetHabitaciones()
        {
            return await _context.Habitaciones.ToListAsync();
        }

        // GET: api/Habitaciones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Habitacion>> GetHabitacion(int id)
        {
            var habitacion = await _context.Habitaciones.FindAsync(id);

            if (habitacion == null)
            {
                return NotFound(new { mensaje = "Habitación no encontrada" });
            }

            return habitacion;
        }

        // GET: api/Habitaciones/hotel/1
        [HttpGet("hotel/{hotelId}")]
        public async Task<ActionResult<IEnumerable<Habitacion>>> GetHabitacionesPorHotel(int hotelId)
        {
            var hotelExiste = await _context.Hoteles.AnyAsync(h => h.Id == hotelId);
            if (!hotelExiste)
            {
                return NotFound(new { mensaje = "Hotel no encontrado" });
            }

            var habitaciones = await _context.Habitaciones
                .Where(h => h.HotelId == hotelId)
                .OrderBy(h => h.NumeroHabitacion)
                .ToListAsync();

            return habitaciones;
        }

        // POST: api/Habitaciones
        [HttpPost]
        public async Task<ActionResult<Habitacion>> PostHabitacion(Habitacion habitacion)
        {
            // Validar que el hotel existe
            var hotelExiste = await _context.Hoteles.AnyAsync(h => h.Id == habitacion.HotelId);
            if (!hotelExiste)
            {
                return BadRequest(new { mensaje = "El hotel especificado no existe" });
            }

            // Validar que no exista ya una habitación con ese número en ese hotel
            var habitacionExiste = await _context.Habitaciones
                .AnyAsync(h => h.HotelId == habitacion.HotelId && h.NumeroHabitacion == habitacion.NumeroHabitacion);

            if (habitacionExiste)
            {
                return BadRequest(new { mensaje = $"Ya existe la habitación {habitacion.NumeroHabitacion} en este hotel" });
            }

            // Validar y normalizar TipoHabitacion
            if (!string.IsNullOrEmpty(habitacion.TipoHabitacion))
            {
                habitacion.TipoHabitacion = habitacion.TipoHabitacion.ToLower();
                var tiposValidos = new[] { "sencilla", "doble", "suite" };
                if (!tiposValidos.Contains(habitacion.TipoHabitacion))
                {
                    return BadRequest(new { mensaje = "TipoHabitacion debe ser: sencilla, doble o suite" });
                }
            }

            // Validar y normalizar EstadoHabitacion
            if (!string.IsNullOrEmpty(habitacion.EstadoHabitacion))
            {
                habitacion.EstadoHabitacion = habitacion.EstadoHabitacion.ToLower();
                var estadosValidos = new[] { "disponible", "mantenimiento", "fueradeservicio" };
                if (!estadosValidos.Contains(habitacion.EstadoHabitacion))
                {
                    return BadRequest(new { mensaje = "EstadoHabitacion debe ser: disponible, mantenimiento o fueradeservicio" });
                }
            }

            _context.Habitaciones.Add(habitacion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetHabitacion), new { id = habitacion.Id }, habitacion);
        }

        // PUT: api/Habitaciones/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHabitacion(int id, Habitacion habitacion)
        {
            if (id != habitacion.Id)
            {
                return BadRequest(new { mensaje = "El ID no coincide" });
            }

            // Verificar que existe
            var habitacionExiste = await _context.Habitaciones.AnyAsync(h => h.Id == id);
            if (!habitacionExiste)
            {
                return NotFound(new { mensaje = "Habitación no encontrada" });
            }

            // Validar y normalizar TipoHabitacion
            if (!string.IsNullOrEmpty(habitacion.TipoHabitacion))
            {
                habitacion.TipoHabitacion = habitacion.TipoHabitacion.ToLower();
                var tiposValidos = new[] { "sencilla", "doble", "suite" };
                if (!tiposValidos.Contains(habitacion.TipoHabitacion))
                {
                    return BadRequest(new { mensaje = "TipoHabitacion debe ser: sencilla, doble o suite" });
                }
            }

            // Validar y normalizar EstadoHabitacion
            if (!string.IsNullOrEmpty(habitacion.EstadoHabitacion))
            {
                habitacion.EstadoHabitacion = habitacion.EstadoHabitacion.ToLower();
                var estadosValidos = new[] { "disponible", "mantenimiento", "fueradeservicio" };
                if (!estadosValidos.Contains(habitacion.EstadoHabitacion))
                {
                    return BadRequest(new { mensaje = "EstadoHabitacion debe ser: disponible, mantenimiento o fueradeservicio" });
                }
            }

            _context.Entry(habitacion).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Habitaciones.AnyAsync(e => e.Id == id))
                {
                    return NotFound(new { mensaje = "Habitación no encontrada" });
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Habitaciones/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHabitacion(int id)
        {
            var habitacion = await _context.Habitaciones.FindAsync(id);

            if (habitacion == null)
            {
                return NotFound(new { mensaje = "Habitación no encontrada" });
            }

            // Verificar si tiene reservas activas
            var tieneReservasActivas = await _context.Reservas
                .AnyAsync(r => r.HabitacionesIds.Contains(id) &&
                              r.EstadoReserva == "activa");

            if (tieneReservasActivas)
            {
                return BadRequest(new { mensaje = "No se puede eliminar la habitación porque tiene reservas activas" });
            }

            _context.Habitaciones.Remove(habitacion);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Habitación eliminada exitosamente" });
        }
    }
}