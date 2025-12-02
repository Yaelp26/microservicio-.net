namespace Travelink.Inventory.Models
{
    public class Reserva
    {
        public int Id { get; set; }
        public int HotelId { get; set; }
        public List<int> HabitacionesIds { get; set; } = new List<int>();
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string EstadoReserva { get; set; } = "activa";  // activa, cancelada, terminada
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public string? ClienteId { get; set; }
        public string? ClienteNombre { get; set; }
        public string? ClienteEmail { get; set; }
    }
}
