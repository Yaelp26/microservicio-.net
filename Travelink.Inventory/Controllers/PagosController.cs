using Microsoft.AspNetCore.Mvc;

namespace Travelink.Inventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PagosController : ControllerBase
    {
        // DTO: Clase para recibir los datos del JSON
        public class PagoRequest
        {
            public int ReservaId { get; set; }
            public decimal Monto { get; set; }
            public string MetodoPago { get; set; } // Ej: "Visa", "PayPal"
        }

        // POST: api/Pagos/capture
        // Simula el cobro de una reserva
        [HttpPost("capture")]
        public IActionResult CapturarPago([FromBody] PagoRequest request)
        {
            // 1. Validación básica
            if (request.Monto <= 0)
            {
                return BadRequest(new { error = "El monto debe ser mayor a 0." });
            }

            if (string.IsNullOrEmpty(request.MetodoPago))
            {
                return BadRequest(new { error = "El método de pago es obligatorio." });
            }

            // 2. Simulación de procesamiento (Aquí conectarías con Stripe/PayPal en la vida real)
            bool pagoExitoso = true; // Simulamos que el banco siempre dice que sí

            if (pagoExitoso)
            {
                // 3. Generar respuesta de éxito
                return Ok(new
                {
                    status = "Pagada",
                    transactionId = Guid.NewGuid().ToString(), // Genera un folio único falso (Ej: a1b2-c3d4...)
                    fecha = DateTime.Now,
                    mensaje = $"Pago de ${request.Monto} recibido correctamente."
                });
            }
            else
            {
                return StatusCode(402, new { error = "Fondos insuficientes" });
            }
        }
    }
}