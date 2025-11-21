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

        // GET: api/Habitaciones/hotel/1
        [HttpGet("hotel/{hotelId}")]
        public async Task<ActionResult<IEnumerable<Habitacion>>> GetHabitacionesPorHotel(int hotelId)
        {
            var habitaciones = await _context.Habitaciones
                                             .Where(h => h.HotelId == hotelId)
                                             .ToListAsync();
            return habitaciones;
        }

        // POST: api/Habitaciones
        // Este endpoint cubre el RF03 "Gestionar habitaciones" y "Tarifas" (al poner el precio)
        [HttpPost]
        public async Task<ActionResult<Habitacion>> PostHabitacion(Habitacion habitacion)
        {
            // Validamos que el hotel exista antes de agregarle cuarto
            var hotelExiste = await _context.Hoteles.AnyAsync(h => h.Id == habitacion.HotelId);
            if (!hotelExiste)
            {
                return BadRequest(new { mensaje = "El Hotel ID especificado no existe." });
            }

            _context.Habitaciones.Add(habitacion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetHabitacionesPorHotel), new { hotelId = habitacion.HotelId }, habitacion);
        }

        // PUT: api/Habitaciones/5
        // Para actualizar precios (RF03 - Gestionar tarifas) o cantidad
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHabitacion(int id, Habitacion habitacion)
        {
            if (id != habitacion.Id) return BadRequest();

            _context.Entry(habitacion).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Habitaciones.Any(e => e.Id == id)) return NotFound();
                else throw;
            }

            return NoContent();
        }
    }
}