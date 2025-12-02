using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Travelink.Inventory.Data;
using Travelink.Inventory.Models;

namespace Travelink.Inventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservasController : ControllerBase
    {
        private readonly InventoryContext _context;

        public ReservasController(InventoryContext context)
        {
            _context = context;
        }

        // GET: api/Reservas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reserva>>> GetReservas()
        {
            return await _context.Reservas
                .OrderByDescending(r => r.FechaCreacion)
                .ToListAsync();
        }

        // GET: api/Reservas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Reserva>> GetReserva(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);

            if (reserva == null)
            {
                return NotFound(new { mensaje = "Reserva no encontrada" });
            }

            return reserva;
        }

        // GET: api/Reservas/cliente/12345
        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<Reserva>>> GetReservasPorCliente(string clienteId)
        {
            var reservas = await _context.Reservas
                .Where(r => r.ClienteId == clienteId)
                .OrderByDescending(r => r.FechaCreacion)
                .ToListAsync();

            return reservas;
        }

        // GET: api/Reservas/hotel/1
        [HttpGet("hotel/{hotelId}")]
        public async Task<ActionResult<IEnumerable<Reserva>>> GetReservasPorHotel(int hotelId)
        {
            var hotelExiste = await _context.Hoteles.AnyAsync(h => h.Id == hotelId);
            if (!hotelExiste)
            {
                return NotFound(new { mensaje = "Hotel no encontrado" });
            }

            var reservas = await _context.Reservas
                .Where(r => r.HotelId == hotelId)
                .OrderByDescending(r => r.FechaCreacion)
                .ToListAsync();

            return reservas;
        }

        // POST: api/Reservas
        [HttpPost]
        public async Task<ActionResult<Reserva>> PostReserva(Reserva reserva)
        {
            // Validar que el hotel existe
            var hotel = await _context.Hoteles.FindAsync(reserva.HotelId);
            if (hotel == null)
            {
                return BadRequest(new { mensaje = "El hotel especificado no existe" });
            }

            // Validar que el hotel está disponible
            if (hotel.EstadoHotel != "disponible")
            {
                return BadRequest(new { mensaje = "El hotel no está disponible para reservas" });
            }

            // Validar fechas
            if (reserva.FechaInicio >= reserva.FechaFin)
            {
                return BadRequest(new { mensaje = "La fecha de inicio debe ser anterior a la fecha de fin" });
            }

            if (reserva.FechaInicio < DateTime.Today)
            {
                return BadRequest(new { mensaje = "La fecha de inicio no puede ser en el pasado" });
            }

            // Validar que hay habitaciones seleccionadas
            if (reserva.HabitacionesIds == null || !reserva.HabitacionesIds.Any())
            {
                return BadRequest(new { mensaje = "Debe seleccionar al menos una habitación" });
            }

            // Validar que todas las habitaciones existen y pertenecen al hotel
            var habitaciones = await _context.Habitaciones
                .Where(h => reserva.HabitacionesIds.Contains(h.Id))
                .ToListAsync();

            if (habitaciones.Count != reserva.HabitacionesIds.Count)
            {
                return BadRequest(new { mensaje = "Una o más habitaciones no existen" });
            }

            var habitacionesDeOtroHotel = habitaciones.Any(h => h.HotelId != reserva.HotelId);
            if (habitacionesDeOtroHotel)
            {
                return BadRequest(new { mensaje = "Todas las habitaciones deben pertenecer al mismo hotel" });
            }

            // Validar que las habitaciones están disponibles (estado)
            var habitacionesNoDisponibles = habitaciones
                .Where(h => h.EstadoHabitacion != "disponible")
                .ToList();

            if (habitacionesNoDisponibles.Any())
            {
                var numeros = string.Join(", ", habitacionesNoDisponibles.Select(h => h.NumeroHabitacion));
                return BadRequest(new { mensaje = $"Las habitaciones {numeros} no están disponibles" });
            }

            // Verificar conflictos con reservas existentes
            var reservasConflictivas = await _context.Reservas
                .Where(r => r.HotelId == reserva.HotelId &&
                           r.EstadoReserva == "activa" &&
                           r.FechaInicio < reserva.FechaFin &&
                           r.FechaFin > reserva.FechaInicio)
                .ToListAsync();

            var habitacionesOcupadas = reservasConflictivas
                .SelectMany(r => r.HabitacionesIds)
                .Distinct()
                .ToList();

            var habitacionesEnConflicto = reserva.HabitacionesIds
                .Where(id => habitacionesOcupadas.Contains(id))
                .ToList();

            if (habitacionesEnConflicto.Any())
            {
                var numerosConflicto = habitaciones
                    .Where(h => habitacionesEnConflicto.Contains(h.Id))
                    .Select(h => h.NumeroHabitacion);
                var numeros = string.Join(", ", numerosConflicto);
                return BadRequest(new { mensaje = $"Las habitaciones {numeros} ya están reservadas para esas fechas" });
            }

            // Validar y normalizar EstadoReserva
            if (string.IsNullOrEmpty(reserva.EstadoReserva))
            {
                reserva.EstadoReserva = "activa";
            }
            else
            {
                reserva.EstadoReserva = reserva.EstadoReserva.ToLower();
                var estadosValidos = new[] { "activa", "cancelada", "terminada" };
                if (!estadosValidos.Contains(reserva.EstadoReserva))
                {
                    return BadRequest(new { mensaje = "EstadoReserva debe ser: activa, cancelada o terminada" });
                }
            }

            // Validar información del cliente
            if (string.IsNullOrWhiteSpace(reserva.ClienteId))
            {
                return BadRequest(new { mensaje = "ClienteId es requerido" });
            }

            if (string.IsNullOrWhiteSpace(reserva.ClienteNombre))
            {
                return BadRequest(new { mensaje = "ClienteNombre es requerido" });
            }

            if (string.IsNullOrWhiteSpace(reserva.ClienteEmail))
            {
                return BadRequest(new { mensaje = "ClienteEmail es requerido" });
            }

            // Establecer fecha de creación
            reserva.FechaCreacion = DateTime.UtcNow;

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReserva), new { id = reserva.Id }, reserva);
        }

        // PUT: api/Reservas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReserva(int id, Reserva reserva)
        {
            if (id != reserva.Id)
            {
                return BadRequest(new { mensaje = "El ID no coincide" });
            }

            // Verificar que existe
            var reservaExistente = await _context.Reservas.FindAsync(id);
            if (reservaExistente == null)
            {
                return NotFound(new { mensaje = "Reserva no encontrada" });
            }

            // Validar fechas
            if (reserva.FechaInicio >= reserva.FechaFin)
            {
                return BadRequest(new { mensaje = "La fecha de inicio debe ser anterior a la fecha de fin" });
            }

            // Validar y normalizar EstadoReserva
            if (!string.IsNullOrEmpty(reserva.EstadoReserva))
            {
                reserva.EstadoReserva = reserva.EstadoReserva.ToLower();
                var estadosValidos = new[] { "activa", "cancelada", "terminada" };
                if (!estadosValidos.Contains(reserva.EstadoReserva))
                {
                    return BadRequest(new { mensaje = "EstadoReserva debe ser: activa, cancelada o terminada" });
                }
            }

            // Preservar la fecha de creación original
            reserva.FechaCreacion = reservaExistente.FechaCreacion;

            _context.Entry(reservaExistente).State = EntityState.Detached;
            _context.Entry(reserva).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Reservas.AnyAsync(e => e.Id == id))
                {
                    return NotFound(new { mensaje = "Reserva no encontrada" });
                }
                throw;
            }

            return NoContent();
        }

        // PATCH: api/Reservas/5/cancelar
        [HttpPatch("{id}/cancelar")]
        public async Task<IActionResult> CancelarReserva(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);

            if (reserva == null)
            {
                return NotFound(new { mensaje = "Reserva no encontrada" });
            }

            if (reserva.EstadoReserva == "cancelada")
            {
                return BadRequest(new { mensaje = "La reserva ya está cancelada" });
            }

            if (reserva.EstadoReserva == "terminada")
            {
                return BadRequest(new { mensaje = "No se puede cancelar una reserva terminada" });
            }

            reserva.EstadoReserva = "cancelada";
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Reserva cancelada exitosamente", reserva });
        }

        // DELETE: api/Reservas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReserva(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);

            if (reserva == null)
            {
                return NotFound(new { mensaje = "Reserva no encontrada" });
            }

            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Reserva eliminada exitosamente" });
        }
    }
}
