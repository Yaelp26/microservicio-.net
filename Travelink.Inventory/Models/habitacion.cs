namespace Travelink.Inventory.Models
{
    public class Habitacion
    {
        public int Id { get; set; }
        public int HotelId { get; set; } 
        public string Tipo { get; set; } 
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
    }
}