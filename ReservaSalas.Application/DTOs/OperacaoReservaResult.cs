namespace ReservaSalas.Application.DTOs
{
	public class OperacaoReservaResult
	{
		public Guid Id { get; init; }
		public bool EmailEnviado { get; init; }
		public string Erro { get; init; }
	}
}
