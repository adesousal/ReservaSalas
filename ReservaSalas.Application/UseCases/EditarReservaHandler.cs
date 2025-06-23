using ReservaSalas.Application.DTOs;
using ReservaSalas.Domain.Enums;
using ReservaSalas.Domain.Interfaces;
using ReservaSalas.Domain.Repositories;

namespace ReservaSalas.Application.UseCases
{
	public class EditarReservaHandler
	{
		private readonly IReservaRepository _reservaRepository;
		private readonly IEmailService _emailService;

		public EditarReservaHandler(IReservaRepository reservaRepository, IEmailService emailService)
		{
			_reservaRepository = reservaRepository;
			_emailService = emailService;
		}

		public async Task<OperacaoReservaResult> HandleAsync(EditarReservaRequest request)
		{
			var reserva = await _reservaRepository.ObterPorIdAsync(request.Id);

			if (reserva == null)
				throw new InvalidOperationException("Reserva não encontrada.");

			if (reserva.Status != StatusReserva.Confirmada)
				throw new InvalidOperationException("Apenas reservas confirmadas podem ser editadas.");

			var conflitos = await _reservaRepository.ObterPorSalaEPeriodoAsync(
				request.Sala,
				request.DataHoraReserva
			);

			if (conflitos.Any(r => r.Id != request.Id))
				throw new InvalidOperationException("Já existe uma reserva para essa sala nesse horário.");

			reserva.Sala = request.Sala;
			reserva.Usuario = request.Usuario;
			reserva.DataHoraReserva = DateTime.SpecifyKind(request.DataHoraReserva, DateTimeKind.Utc);

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

			return new OperacaoReservaResult { Id = request.Id, EmailEnviado = emailEnviado, Erro = erro };
		}
	}
}
