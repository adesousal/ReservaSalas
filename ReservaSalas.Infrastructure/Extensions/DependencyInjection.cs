using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReservaSalas.Application.UseCases;
using ReservaSalas.Domain.Interfaces;
using ReservaSalas.Domain.Repositories;
using ReservaSalas.Infrastructure.Persistence;
using ReservaSalas.Infrastructure.Repositories;
using ReservaSalas.Infrastructure.Services;

namespace ReservaSalas.Infrastructure.Extensions
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddDbContext<ReservaDbContext>(options =>
				options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

			services.AddScoped<IReservaRepository, ReservaRepository>();
			services.AddScoped<IEmailService, EmailService>();

			return services;
		}

		public static IServiceCollection AddApplication(this IServiceCollection services)
		{
			services.AddScoped<CriarReservaHandler>();
			services.AddScoped<EditarReservaHandler>();
			services.AddScoped<CancelarReservaHandler>();
			services.AddScoped<ListarReservasHandler>();

			return services;
		}
	}
}
