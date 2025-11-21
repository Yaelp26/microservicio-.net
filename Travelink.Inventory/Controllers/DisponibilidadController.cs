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

        [HttpGet]
        public async Task<IActionResult> ConsultarDisponibilidad(int hotelId, string tipo)
        {
            var habitaciones = await _context.Habitaciones
                .Where(h => h.HotelId == hotelId && h.Tipo == tipo)
                .ToListAsync();

            if (!habitaciones.Any())
            {
                // Si el endpoint existe pero no hay cuartos, devuelve esto:
                return NotFound(new { mensaje = "No se encontraron habitaciones de ese tipo." });
            }

            var disponibles = habitaciones.Where(h => h.Cantidad > 0).ToList();

            if (disponibles.Any())
            {
                return Ok(new
                {
                    disponible = true,
                    cantidad = disponibles.Sum(h => h.Cantidad),
                    precio = disponibles.First().Precio
                });
            }

            return Ok(new { disponible = false, mensaje = "Agotado" });
        }
    }
}