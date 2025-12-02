using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Travelink.Inventory.Data;
using Travelink.Inventory.Models;

namespace Travelink.Inventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DisponibilidadController : ControllerBase
    {
        private readonly InventoryContext _context;

        public DisponibilidadController(InventoryContext context)
        {
            _context = context;
        }

        // GET: api/Disponibilidad?hotelId=1&tipoHabitacion=doble&fechaInicio=2024-12-01&fechaFin=2024-12-05
        [HttpGet]
        public async Task<IActionResult> ConsultarDisponibilidad(
            int hotelId,
            string tipoHabitacion,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            // Validar que el hotel existe
            var hotelExiste = await _context.Hoteles.AnyAsync(h => h.Id == hotelId);
            if (!hotelExiste)
            {
                return NotFound(new { mensaje = "Hotel no encontrado" });
            }

            // Validar fechas
            if (fechaInicio >= fechaFin)
            {
                return BadRequest(new { mensaje = "La fecha de inicio debe ser anterior a la fecha de fin" });
            }

            if (fechaInicio < DateTime.Today)
            {
                return BadRequest(new { mensaje = "La fecha de inicio no puede ser en el pasado" });
            }

            // Normalizar y validar tipo de habitación
            tipoHabitacion = tipoHabitacion.ToLower();
            var tiposValidos = new[] { "sencilla", "doble", "suite" };
            if (!tiposValidos.Contains(tipoHabitacion))
            {
                return BadRequest(new { mensaje = "TipoHabitacion debe ser: sencilla, doble o suite" });
            }

            // Obtener todas las habitaciones del tipo solicitado en el hotel
            var habitaciones = await _context.Habitaciones
                .Where(h => h.HotelId == hotelId &&
                           h.TipoHabitacion == tipoHabitacion &&
                           h.EstadoHabitacion == "disponible")
                .ToListAsync();

            if (!habitaciones.Any())
            {
                return NotFound(new { mensaje = $"No se encontraron habitaciones de tipo {tipoHabitacion} disponibles en este hotel" });
            }

            // Obtener reservas activas que se solapan con las fechas solicitadas
            var reservasActivas = await _context.Reservas
                .Where(r => r.HotelId == hotelId &&
                           r.EstadoReserva == "activa" &&
                           r.FechaInicio < fechaFin &&
                           r.FechaFin > fechaInicio)
                .ToListAsync();

            // Obtener IDs de habitaciones reservadas
            var habitacionesReservadas = reservasActivas
                .SelectMany(r => r.HabitacionesIds)
                .Distinct()
                .ToList();

            // Filtrar habitaciones disponibles (no reservadas)
            var habitacionesDisponibles = habitaciones
                .Where(h => !habitacionesReservadas.Contains(h.Id))
                .ToList();

            if (habitacionesDisponibles.Any())
            {
                // Obtener el precio más común o el primero
                var precioRepresentativo = habitacionesDisponibles
                    .GroupBy(h => h.Precio)
                    .OrderByDescending(g => g.Count())
                    .First()
                    .Key;

                return Ok(new
                {
                    disponible = true,
                    cantidad = habitacionesDisponibles.Count,
                    precio = precioRepresentativo,
                    habitaciones = habitacionesDisponibles.Select(h => new
                    {
                        id = h.Id,
                        numeroHabitacion = h.NumeroHabitacion,
                        precio = h.Precio,
                        imagenes = h.Imagenes
                    })
                });
            }

            return Ok(new
            {
                disponible = false,
                mensaje = "No hay habitaciones disponibles para las fechas seleccionadas",
                totalHabitaciones = habitaciones.Count
            });
        }
    }
}