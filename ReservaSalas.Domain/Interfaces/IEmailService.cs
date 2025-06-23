namespace ReservaSalas.Domain.Interfaces
{
	public interface IEmailService
	{
		Task EnviarConfirmacaoReservaAsync(string destinatario, string sala, DateTime dataHora);

		Task EnviarCancelamentoReservaAsync(string destinatario, string sala, DateTime dataHora);
	}
}
