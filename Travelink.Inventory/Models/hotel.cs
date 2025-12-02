namespace Travelink.Inventory.Models
{
    public class Hotel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Ciudad { get; set; }
        public string Direccion { get; set; } // iframe de Google Maps
        public bool Activo { get; set; } = true;

        // Lista de URLs de imágenes almacenadas en MinIO
        public List<string> Imagenes { get; set; } = new List<string>();
    }
}