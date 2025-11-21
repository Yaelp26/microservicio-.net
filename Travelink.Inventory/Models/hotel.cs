namespace Travelink.Inventory.Models
{
    public class Hotel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public bool Activo { get; set; } = true;
    }
}