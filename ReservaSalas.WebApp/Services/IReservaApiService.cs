using ReservaSalas.Application.DTOs;

namespace ReservaSalas.WebApp.Services
{
	public interface IReservaApiService
	{
		Task<PagedResult<ReservaResponse>> ListarAsync(ReservasFilter filtro);
		Task<ReservaResponse> ObterPorIdAsync(Guid id);
		Task CriarAsync(CriarReservaRequest request);
		Task EditarAsync(EditarReservaRequest request);
		Task CancelarAsync(Guid id);
	}
}
