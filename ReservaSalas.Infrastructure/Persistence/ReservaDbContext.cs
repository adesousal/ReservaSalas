using Microsoft.EntityFrameworkCore;
using ReservaSalas.Domain.Entities;

namespace ReservaSalas.Infrastructure.Persistence
{
	public class ReservaDbContext : DbContext
	{
		public ReservaDbContext(DbContextOptions<ReservaDbContext> options) : base(options)
		{
		}

		public DbSet<Reserva> Reservas => Set<Reserva>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Reserva>(entity =>
			{
				entity.HasKey(r => r.Id);

				entity.Property(r => r.Sala)
					  .IsRequired()
					  .HasMaxLength(100);

				entity.Property(r => r.Usuario)
					  .IsRequired()
					  .HasMaxLength(100);

				entity.Property(r => r.DataHoraReserva)
					  .IsRequired();

				entity.Property(r => r.Status)
					  .IsRequired();
			});
		}
	}
}
