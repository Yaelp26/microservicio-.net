using Microsoft.EntityFrameworkCore;
using Travelink.Inventory.Models;

namespace Travelink.Inventory.Data
{
    public class InventoryContext : DbContext
    {
        public InventoryContext(DbContextOptions<InventoryContext> options) : base(options) { }

        public DbSet<Hotel> Hoteles { get; set; }
        public DbSet<Habitacion> Habitaciones { get; set; }
        public DbSet<Reserva> Reservas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar las listas como JSON en PostgreSQL
            modelBuilder.Entity<Hotel>()
                .Property(h => h.Imagenes)
                .HasColumnType("jsonb");

            modelBuilder.Entity<Habitacion>()
                .Property(h => h.Imagenes)
                .HasColumnType("jsonb");

            modelBuilder.Entity<Reserva>()
                .Property(r => r.HabitacionesIds)
                .HasColumnType("jsonb");
        }
    }
}