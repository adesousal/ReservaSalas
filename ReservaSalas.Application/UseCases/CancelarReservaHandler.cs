using ReservaSalas.Application.DTOs;
using ReservaSalas.Domain.Enums;
using ReservaSalas.Domain.Interfaces;
using ReservaSalas.Domain.Repositories;

namespace ReservaSalas.Application.UseCases
{
	public class CancelarReservaHandler
	{
		private readonly IReservaRepository _reservaRepository;
		private readonly IEmailService _emailService;

		public CancelarReservaHandler(IReservaRepository reservaRepository, IEmailService emailService)
		{
			_reservaRepository = reservaRepository;
			_emailService = emailService;
		}

		public async Task<OperacaoReservaResult> HandleAsync(Guid id)
		{
			var reserva = await _reservaRepository.ObterPorIdAsync(id);

			if (reserva == null)
				throw new InvalidOperationException("Reserva não encontrada.");

			if (!reserva.PodeSerCancelada())
				throw new InvalidOperationException("A reserva só pode ser cancelada com no mínimo 24 horas de antecedência.");

			reserva.Status = StatusReserva.Cancelada;

			_reservaRepository.Atualizar(reserva);
			await _reservaRepository.SalvarAlteracoesAsync();

			bool emailEnviado = true;
			string erro = "";
			try
			{
				await _emailService.EnviarCancelamentoReservaAsync(
					reserva.Usuario,
					reserva.Sala,
					reserva.DataHoraReserva);
			}
			catch(Exception ex)
			{
				emailEnviado = false;
				erro = ex.Message;
			}

			return new OperacaoReservaResult { Id = id, EmailEnviado = emailEnviado, Erro = erro };
		}
	}
}
