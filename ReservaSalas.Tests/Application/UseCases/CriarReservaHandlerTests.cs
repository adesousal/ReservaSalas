using Moq;
using ReservaSalas.Application.DTOs;
using ReservaSalas.Application.UseCases;
using ReservaSalas.Domain.Entities;
using ReservaSalas.Domain.Interfaces;
using ReservaSalas.Domain.Repositories;

namespace ReservaSalas.Tests.Application.UseCases
{
	public class CriarReservaHandlerTests
	{
		private readonly Mock<IReservaRepository> _repoMock = new();
		private readonly Mock<IEmailService> _emailMock = new();
		private readonly CriarReservaHandler _handler;

		public CriarReservaHandlerTests()
		{
			_handler = new CriarReservaHandler(_repoMock.Object, _emailMock.Object);
		}

		[Fact]
		public async Task HandleAsync_SemConflito_EmailSucesso_Deve_Retornar_Result_Sucesso()
		{
			// Arrange
			var request = new CriarReservaRequest
			{
				Sala = "Sala A",
				Usuario = "user@ex.com",
				DataHoraReserva = DateTime.UtcNow.AddHours(3)
			};
			_repoMock
				.Setup(r => r.ObterPorSalaEPeriodoAsync(request.Sala, request.DataHoraReserva))
				.ReturnsAsync(new List<Reserva>()); // sem conflito

			Reserva reservaAdicionada = null!;
			_repoMock
				.Setup(r => r.AdicionarAsync(It.IsAny<Reserva>()))
				.Callback<Reserva>(r => reservaAdicionada = r)
				.Returns(Task.CompletedTask);
			_repoMock
				.Setup(r => r.SalvarAlteracoesAsync())
				.Returns(Task.CompletedTask);

			// emailMock sem setup causa sucesso (nenhuma exceção)

			// Act
			var result = await _handler.HandleAsync(request);

			// Assert
			Assert.NotNull(reservaAdicionada);
			Assert.Equal(reservaAdicionada.Id, result.Id);
			Assert.True(result.EmailEnviado);
			Assert.True(string.IsNullOrEmpty(result.Erro));

			_repoMock.Verify(r => r.AdicionarAsync(reservaAdicionada), Times.Once);
			_repoMock.Verify(r => r.SalvarAlteracoesAsync(), Times.Once);
			_emailMock.Verify(e => e.EnviarConfirmacaoReservaAsync(
				reservaAdicionada.Usuario,
				reservaAdicionada.Sala,
				reservaAdicionada.DataHoraReserva), Times.Once);
		}

		[Fact]
		public async Task HandleAsync_SemConflito_EmailFalha_Deve_Retornar_Result_ComErro()
		{
			// Arrange
			var request = new CriarReservaRequest
			{
				Sala = "Sala B",
				Usuario = "fail@ex.com",
				DataHoraReserva = DateTime.UtcNow.AddHours(4)
			};
			_repoMock
				.Setup(r => r.ObterPorSalaEPeriodoAsync(request.Sala, request.DataHoraReserva))
				.ReturnsAsync(new List<Reserva>());

			_repoMock.Setup(r => r.AdicionarAsync(It.IsAny<Reserva>())).Returns(Task.CompletedTask);
			_repoMock.Setup(r => r.SalvarAlteracoesAsync()).Returns(Task.CompletedTask);

			var emailEx = new Exception("SMTP down");
			_emailMock
				.Setup(e => e.EnviarConfirmacaoReservaAsync(
					It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
				.ThrowsAsync(emailEx);

			// Act
			var result = await _handler.HandleAsync(request);

			// Assert
			Assert.False(result.EmailEnviado);
			Assert.Equal(emailEx.Message, result.Erro);

			_repoMock.Verify(r => r.AdicionarAsync(It.IsAny<Reserva>()), Times.Once);
			_repoMock.Verify(r => r.SalvarAlteracoesAsync(), Times.Once);
			_emailMock.Verify(e => e.EnviarConfirmacaoReservaAsync(
				request.Usuario, request.Sala, It.IsAny<DateTime>()), Times.Once);
		}

		[Fact]
		public async Task HandleAsync_ComConflito_Deve_LancarInvalidOperationException()
		{
			// Arrange
			var request = new CriarReservaRequest
			{
				Sala = "Sala C",
				Usuario = "user@ex.com",
				DataHoraReserva = DateTime.UtcNow.AddHours(5)
			};
			_repoMock
				.Setup(r => r.ObterPorSalaEPeriodoAsync(request.Sala, request.DataHoraReserva))
				.ReturnsAsync(new List<Reserva> { new Reserva() }); // conflito

			// Act & Assert
			var ex = await Assert.ThrowsAsync<InvalidOperationException>(
				() => _handler.HandleAsync(request));
			Assert.Equal("Já existe uma reserva para essa sala nesse horário.", ex.Message);

			_repoMock.Verify(r => r.AdicionarAsync(It.IsAny<Reserva>()), Times.Never);
			_repoMock.Verify(r => r.SalvarAlteracoesAsync(), Times.Never);
			_emailMock.Verify(e => e.EnviarConfirmacaoReservaAsync(
				It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
		}
	}
}
