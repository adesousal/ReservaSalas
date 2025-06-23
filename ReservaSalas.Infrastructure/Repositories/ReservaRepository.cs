using Microsoft.EntityFrameworkCore;
using ReservaSalas.Domain.Entities;
using ReservaSalas.Domain.Enums;
using ReservaSalas.Domain.Filters;
using ReservaSalas.Domain.Repositories;
using ReservaSalas.Infrastructure.Persistence;
using System.Globalization;
using System.Text;

namespace ReservaSalas.Infrastructure.Repositories
{
	public class ReservaRepository : IReservaRepository
	{
		private readonly ReservaDbContext _context;

		public ReservaRepository(ReservaDbContext context)
		{
			_context = context;
		}

		public async Task<Reserva?> ObterPorIdAsync(Guid id)
		{
			return await _context.Reservas.FindAsync(id);
		}

		public async Task<IEnumerable<Reserva>> ObterTodasAsync()
		{
			return await _context.Reservas.ToListAsync();
		}

		public async Task<IEnumerable<Reserva>> ObterPorSalaEPeriodoAsync(string sala, DateTime dataInicio)
		{
			var utcStart = DateTime.SpecifyKind(dataInicio, DateTimeKind.Utc);
			var utcEnd = DateTime.SpecifyKind(dataInicio.AddHours(1), DateTimeKind.Utc);

			// Primeiro filtro por data no SQL
			var preliminar = await _context.Reservas
				.Where(r => r.DataHoraReserva.AddHours(1) > utcStart && r.DataHoraReserva < utcEnd && r.Status == StatusReserva.Confirmada)
				.ToListAsync(); // Neste momento, carrego na memória mesmo, pois é para teste, mas para produção usaria PostgreSQL + unaccent

			// Agora remova acentos e compare em memória
			var termo = RemoverAcentos(sala).ToLowerInvariant();
			return preliminar
				.Where(r => RemoverAcentos(r.Sala).ToLowerInvariant() == termo)
				.ToList();
		}

		public async Task AdicionarAsync(Reserva reserva)
		{
			await _context.Reservas.AddAsync(reserva);
		}

		public void Atualizar(Reserva reserva)
		{
			_context.Reservas.Update(reserva);
		}

		public void Remover(Reserva reserva)
		{
			_context.Reservas.Remove(reserva);
		}

		public async Task SalvarAlteracoesAsync()
		{
			await _context.SaveChangesAsync();
		}

		public async Task<(IEnumerable<Reserva>, int totalCount)> ObterTodasPaginadasAsync(int pageNumber, int pageSize)
		{
			var query = _context.Reservas.AsQueryable();

			var totalCount = await query.CountAsync();

			var items = await query
				.OrderBy(r => r.DataHoraReserva)
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return (items, totalCount);
		}

		public async Task<(IEnumerable<Reserva>, int totalCount)> ObterReservasFiltradasAsync(ReservaFilter filtro)
		{
			var query = _context.Reservas.AsQueryable().AsNoTracking();

			//Caso não seja informada data no filtro, retornará apenas os registros a partir da data corrente.
			var utcStart = DateTime.SpecifyKind(DateTime.Now.Date, DateTimeKind.Utc);

			if (filtro.DataInicio.HasValue)
			{
				utcStart = DateTime.SpecifyKind(filtro.DataInicio.Value, DateTimeKind.Utc);				
			}

			query = query.Where(r => r.DataHoraReserva >= utcStart);

			if (filtro.DataFim.HasValue)
			{
				var utcEnd = DateTime.SpecifyKind(filtro.DataFim.Value.AddDays(1).AddSeconds(-1), DateTimeKind.Utc);
				query = query.Where(r => r.DataHoraReserva <= utcEnd);
			}				

			// Neste momento, carrego na memória mesmo, pois é para teste, mas para produção usaria PostgreSQL + unaccent
			var items = await query.ToListAsync();

			if (!string.IsNullOrEmpty(filtro.Sala))
			{
				var sala = RemoverAcentos(filtro.Sala).ToLowerInvariant();

				items = items
						.Where(r => RemoverAcentos(r.Sala).ToLowerInvariant() == sala)
						.ToList();
			}

			if (!string.IsNullOrEmpty(filtro.Usuario))
			{
				var usuario = RemoverAcentos(filtro.Usuario).ToLowerInvariant();

				items = items
						.Where(r => RemoverAcentos(r.Usuario).ToLowerInvariant() == usuario)
						.ToList();
			}

			var totalCount = items.Count();

			items = items
					.OrderBy(r => r.DataHoraReserva)
					.Skip((filtro.PageNumber - 1) * filtro.PageSize)
					.Take(filtro.PageSize)
					.ToList();

			return (items, totalCount);
		}

		static string RemoverAcentos(string text)
		{
			var stFormD = text.Normalize(System.Text.NormalizationForm.FormD);
			return new string(stFormD
				.Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
				.ToArray()).Normalize(NormalizationForm.FormC).Trim().ToLower();
		}
	}
}
