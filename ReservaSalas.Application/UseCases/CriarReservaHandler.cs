using ReservaSalas.Application.DTOs;
using ReservaSalas.Domain.Entities;
using ReservaSalas.Domain.Interfaces;
using ReservaSalas.Domain.Repositories;

namespace ReservaSalas.Application.UseCases
{
	public class CriarReservaHandler
	{
		private readonly IReservaRepository _reservaRepository;
		private readonly IEmailService _emailService;

		public CriarReservaHandler(IReservaRepository reservaRepository, IEmailService emailService)
		{
			_reservaRepository = reservaRepository;
			_emailService = emailService;
		}

		public async Task<OperacaoReservaResult> HandleAsync(CriarReservaRequest request)
		{
			// Regra de conflito: verificar se já existe reserva para mesma sala e hora
			var conflitos = await _reservaRepository.ObterPorSalaEPeriodoAsync(
				request.Sala,
				request.DataHoraReserva 
			);

			if (conflitos.Any())
				throw new InvalidOperationException("Já existe uma reserva para essa sala nesse horário.");

			var novaReserva = new Reserva
			{
				Id = Guid.NewGuid(),
				Sala = request.Sala,
				Usuario = request.Usuario,
				DataHoraReserva = DateTime.SpecifyKind(request.DataHoraReserva, DateTimeKind.Utc),
				Status = Domain.Enums.StatusReserva.Confirmada
			};

			await _reservaRepository.AdicionarAsync(novaReserva);
			await _reservaRepository.SalvarAlteracoesAsync();

			bool emailEnviado = true;
			string erro = "";
			try
			{
				await _emailService.EnviarConfirmacaoReservaAsync(
					novaReserva.Usuario,
					novaReserva.Sala,
					novaReserva.DataHoraReserva);
			}
			catch(Exception ex)
			{
				emailEnviado = false;
				erro = ex.Message;
			}

			return new OperacaoReservaResult { Id = novaReserva.Id, EmailEnviado = emailEnviado, Erro = erro };
		}
	}
}
