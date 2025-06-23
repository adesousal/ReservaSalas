using ReservaSalas.Domain.Entities;
using ReservaSalas.Domain.Filters;

namespace ReservaSalas.Domain.Repositories
{
	public interface IReservaRepository
	{
		Task<Reserva?> ObterPorIdAsync(Guid id);
		Task<IEnumerable<Reserva>> ObterTodasAsync();
		Task<IEnumerable<Reserva>> ObterPorSalaEPeriodoAsync(string sala, DateTime dataInicio);
		Task AdicionarAsync(Reserva reserva);
		void Atualizar(Reserva reserva);
		void Remover(Reserva reserva);
		Task SalvarAlteracoesAsync();
		Task<(IEnumerable<Reserva>, int totalCount)> ObterTodasPaginadasAsync(int pageNumber, int pageSize);
		Task<(IEnumerable<Reserva>, int totalCount)> ObterReservasFiltradasAsync(ReservaFilter filtro);
	}
}
