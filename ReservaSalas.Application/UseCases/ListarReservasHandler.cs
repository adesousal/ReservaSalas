using ReservaSalas.Application.DTOs;
using ReservaSalas.Domain.Entities;
using ReservaSalas.Domain.Filters;
using ReservaSalas.Domain.Repositories;

namespace ReservaSalas.Application.UseCases
{
	public class ListarReservasHandler
	{
		private readonly IReservaRepository _reservaRepository;

		public ListarReservasHandler(IReservaRepository reservaRepository)
		{
			_reservaRepository = reservaRepository;
		}

		public async Task<PagedResult<ReservaResponse>> HandleAsync(ReservasFilter filtro)
		{
			// Mapear DTO Application para filtro Domain
			var filtroDomain = new ReservaFilter
			{
				Sala = filtro.Sala,
				Usuario = filtro.Usuario,
				DataInicio = filtro.DataInicio,
				DataFim = filtro.DataFim,
				PageNumber = filtro.PageNumber,
				PageSize = filtro.PageSize
			};

			var (reservas, totalCount) = await _reservaRepository.ObterReservasFiltradasAsync(filtroDomain);

			var items = reservas.Select(r => new ReservaResponse
			{
				Id = r.Id,
				Sala = r.Sala,
				Usuario = r.Usuario,
				DataHoraReserva = r.DataHoraReserva,
				Status = r.Status
			});

			return new PagedResult<ReservaResponse>
			{
				Items = items,
				TotalCount = totalCount,
				PageNumber = filtro.PageNumber,
				PageSize = filtro.PageSize
			};
		}

		public async Task<Reserva?> ObterPorIdAsync(Guid id)
		{
			var reserva = await _reservaRepository.ObterPorIdAsync(id);
			return reserva;
		}
	}
}
