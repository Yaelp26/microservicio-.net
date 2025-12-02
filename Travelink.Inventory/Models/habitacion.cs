namespace Travelink.Inventory.Models
{
    public class Habitacion
    {
        public int Id { get; set; }  // ID global en base de datos
        public int HotelId { get; set; }
        public int NumeroHabitacion { get; set; }  // Número de habitación dentro del hotel
        public string TipoHabitacion { get; set; } = string.Empty;  // sencilla, doble, suite
        public decimal Precio { get; set; }
        public string EstadoHabitacion { get; set; } = "disponible"; // disponible, mantenimiento, fueradeservicio
        public List<string> Imagenes { get; set; } = new List<string>();
    }
}