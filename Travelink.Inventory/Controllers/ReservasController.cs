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

        // POST: api/Reservas
        // Este endpoint será llamado por Laravel cuando se crea una reserva
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

            return CreatedAtAction(nameof(PostReserva), new { id = reserva.Id }, reserva);
        }

        // PATCH: api/Reservas/5/estado
        // Laravel llama este endpoint para actualizar el estado de una reserva
        [HttpPatch("{id}/estado")]
        public async Task<IActionResult> ActualizarEstadoReserva(int id, [FromBody] ActualizarEstadoRequest request)
        {
            var reserva = await _context.Reservas.FindAsync(id);

            if (reserva == null)
            {
                return NotFound(new { mensaje = "Reserva no encontrada" });
            }

            // Validar y normalizar estado
            if (string.IsNullOrEmpty(request.Estado))
            {
                return BadRequest(new { mensaje = "Estado es requerido" });
            }

            var estadoNormalizado = request.Estado.ToLower();
            var estadosValidos = new[] { "activa", "cancelada", "terminada" };

            if (!estadosValidos.Contains(estadoNormalizado))
            {
                return BadRequest(new { mensaje = "Estado debe ser: activa, cancelada o terminada" });
            }

            reserva.EstadoReserva = estadoNormalizado;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = $"Reserva actualizada a estado: {estadoNormalizado}", reserva });
        }

        // PATCH: api/Reservas/5/cancelar
        // Atajo para cancelar (alternativa al endpoint de estado)
        [HttpPatch("{id}/cancelar")]
        public async Task<IActionResult> CancelarReserva(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);

            if (reserva == null)
            {
                return NotFound(new { mensaje = "Reserva no encontrada" });
            }

            reserva.EstadoReserva = "cancelada";
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Reserva cancelada exitosamente", reserva });
        }

        // DELETE: api/Reservas/5
        // Solo para administradores - limpieza de datos
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

    // Modelo para el request de actualización de estado
    public class ActualizarEstadoRequest
    {
        public string Estado { get; set; } = string.Empty;
    }
}
