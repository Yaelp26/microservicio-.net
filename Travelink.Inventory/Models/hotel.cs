namespace Travelink.Inventory.Models
{
    public class Hotel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;  // iframe de Google Maps
        public string EstadoHotel { get; set; } = "disponible";
        public List<string> Imagenes { get; set; } = new List<string>();  // URLs de imágenes
    }
}