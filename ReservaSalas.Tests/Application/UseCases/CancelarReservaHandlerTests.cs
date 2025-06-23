using Moq;
using ReservaSalas.Application.UseCases;
using ReservaSalas.Domain.Entities;
using ReservaSalas.Domain.Enums;
using ReservaSalas.Domain.Interfaces;
using ReservaSalas.Domain.Repositories;

namespace ReservaSalas.Tests.Application.UseCases
{
	public class CancelarReservaHandlerTests
	{
		private readonly Mock<IReservaRepository> _repoMock = new();
		private readonly Mock<IEmailService> _emailMock = new();
		private readonly CancelarReservaHandler _handler;

		public CancelarReservaHandlerTests()
		{
			_handler = new CancelarReservaHandler(_repoMock.Object, _emailMock.Object);
		}

		[Fact]
		public async Task HandleAsync_Sucesso_EmailSucesso_Deve_Retornar_Result_Sucesso()
		{
			// Arrange
			var id = Guid.NewGuid();
			var futura = DateTime.UtcNow.AddDays(2);
			var reserva = new Reserva
			{
				Id = id,
				Sala = "SalaX",
				Usuario = "u@ex.com",
				DataHoraReserva = futura,
				Status = StatusReserva.Confirmada
			};
			_repoMock.Setup(r => r.ObterPorIdAsync(id))
					 .ReturnsAsync(reserva);
			_repoMock.Setup(r => r.Atualizar(reserva));
			_repoMock.Setup(r => r.SalvarAlteracoesAsync())
					 .Returns(Task.CompletedTask);

			// Act
			var result = await _handler.HandleAsync(id);

			// Assert
			Assert.Equal(id, result.Id);
			Assert.True(result.EmailEnviado);
			Assert.True(string.IsNullOrEmpty(result.Erro));

			_repoMock.Verify(r => r.Atualizar(reserva), Times.Once);
			_repoMock.Verify(r => r.SalvarAlteracoesAsync(), Times.Once);
			_emailMock.Verify(e => e.EnviarCancelamentoReservaAsync(
				reserva.Usuario, reserva.Sala, reserva.DataHoraReserva), Times.Once);
		}

		[Fact]
		public async Task HandleAsync_Sucesso_EmailFalha_Deve_Retornar_Result_ComErro()
		{
			// Arrange
			var id = Guid.NewGuid();
			var futura = DateTime.UtcNow.AddDays(2);
			var reserva = new Reserva
			{
				Id = id,
				Sala = "SalaY",
				Usuario = "y@ex.com",
				DataHoraReserva = futura,
				Status = StatusReserva.Confirmada
			};
			_repoMock.Setup(r => r.ObterPorIdAsync(id))
					 .ReturnsAsync(reserva);
			_repoMock.Setup(r => r.Atualizar(reserva));
			_repoMock.Setup(r => r.SalvarAlteracoesAsync())
					 .Returns(Task.CompletedTask);

			var emailEx = new Exception("SMTP down");
			_emailMock
				.Setup(e => e.EnviarCancelamentoReservaAsync(
					It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
				.ThrowsAsync(emailEx);

			// Act
			var result = await _handler.HandleAsync(id);

			// Assert
			Assert.Equal(id, result.Id);
			Assert.False(result.EmailEnviado);
			Assert.Equal(emailEx.Message, result.Erro);

			_emailMock.Verify(e => e.EnviarCancelamentoReservaAsync(
				reserva.Usuario, reserva.Sala, reserva.DataHoraReserva), Times.Once);
		}

		[Fact]
		public async Task HandleAsync_Deve_Lancar_Se_ReservaNaoEncontrada()
		{
			// Arrange
			var id = Guid.NewGuid();
			_repoMock.Setup(r => r.ObterPorIdAsync(id))
					 .ReturnsAsync((Reserva?)null);

			// Act & Assert
			var ex = await Assert.ThrowsAsync<InvalidOperationException>(
				() => _handler.HandleAsync(id));
			Assert.Equal("Reserva não encontrada.", ex.Message);

			_repoMock.Verify(r => r.Atualizar(It.IsAny<Reserva>()), Times.Never);
			_repoMock.Verify(r => r.SalvarAlteracoesAsync(), Times.Never);
			_emailMock.Verify(e => e.EnviarCancelamentoReservaAsync(
				It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
		}

		[Fact]
		public async Task HandleAsync_Deve_Lancar_Se_MenosDe24h()
		{
			// Arrange
			var id = Guid.NewGuid();
			var breve = DateTime.UtcNow.AddHours(10); // dentro de 24h
			var reserva = new Reserva
			{
				Id = id,
				Sala = "SalaZ",
				Usuario = "z@ex.com",
				DataHoraReserva = breve,
				Status = StatusReserva.Confirmada
			};
			_repoMock.Setup(r => r.ObterPorIdAsync(id))
					 .ReturnsAsync(reserva);

			// Act & Assert
			var ex = await Assert.ThrowsAsync<InvalidOperationException>(
				() => _handler.HandleAsync(id));
			Assert.Equal("A reserva só pode ser cancelada com no mínimo 24 horas de antecedência.", ex.Message);

			_repoMock.Verify(r => r.Atualizar(It.IsAny<Reserva>()), Times.Never);
			_repoMock.Verify(r => r.SalvarAlteracoesAsync(), Times.Never);
			_emailMock.Verify(e => e.EnviarCancelamentoReservaAsync(
				It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
		}
	}
}
