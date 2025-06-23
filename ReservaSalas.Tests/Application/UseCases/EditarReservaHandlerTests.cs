using Moq;
using ReservaSalas.Application.DTOs;
using ReservaSalas.Application.UseCases;
using ReservaSalas.Domain.Entities;
using ReservaSalas.Domain.Enums;
using ReservaSalas.Domain.Interfaces;
using ReservaSalas.Domain.Repositories;

namespace ReservaSalas.Tests.Application.UseCases
{
	public class EditarReservaHandlerTests
	{
		private readonly Mock<IReservaRepository> _repoMock = new();
		private readonly Mock<IEmailService> _emailMock = new();
		private readonly EditarReservaHandler _handler;

		public EditarReservaHandlerTests()
		{
			_handler = new EditarReservaHandler(_repoMock.Object, _emailMock.Object);
		}

		[Fact]
		public async Task HandleAsync_Sucesso_EmailSucesso_Deve_Retornar_Result_Sucesso()
		{
			// Arrange
			var id = Guid.NewGuid();
			var original = DateTime.UtcNow.AddDays(1);
			var reserva = new Reserva
			{
				Id = id,
				Sala = "Old Sala",
				Usuario = "old@ex.com",
				DataHoraReserva = original,
				Status = StatusReserva.Confirmada
			};

			_repoMock.Setup(r => r.ObterPorIdAsync(id))
					 .ReturnsAsync(reserva);

			_repoMock.Setup(r => r.ObterPorSalaEPeriodoAsync(
					It.IsAny<string>(), It.IsAny<DateTime>()))
					 .ReturnsAsync(new List<Reserva>()); // sem conflito

			_repoMock.Setup(r => r.Atualizar(reserva));
			_repoMock.Setup(r => r.SalvarAlteracoesAsync())
					 .Returns(Task.CompletedTask);

			var request = new EditarReservaRequest
			{
				Id = id,
				Sala = "New Sala",
				Usuario = "new@ex.com",
				DataHoraReserva = DateTime.UtcNow.AddDays(2)
			};

			// Act
			var result = await _handler.HandleAsync(request);

			// Assert
			Assert.Equal(request.Id, result.Id);
			Assert.True(result.EmailEnviado);
			Assert.True(string.IsNullOrEmpty(result.Erro));

			// Confirma que a entidade foi atualizada
			Assert.Equal("New Sala", reserva.Sala);
			Assert.Equal("new@ex.com", reserva.Usuario);
			Assert.Equal(DateTimeKind.Utc, reserva.DataHoraReserva.Kind);
			Assert.Equal(request.DataHoraReserva, reserva.DataHoraReserva);

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
			var reserva = new Reserva
			{
				Id = id,
				Sala = "Sala X",
				Usuario = "x@ex.com",
				DataHoraReserva = DateTime.UtcNow.AddDays(1),
				Status = StatusReserva.Confirmada
			};
			_repoMock.Setup(r => r.ObterPorIdAsync(id))
					 .ReturnsAsync(reserva);
			_repoMock.Setup(r => r.ObterPorSalaEPeriodoAsync(
					It.IsAny<string>(), It.IsAny<DateTime>()))
					 .ReturnsAsync(new List<Reserva>());
			_repoMock.Setup(r => r.Atualizar(reserva));
			_repoMock.Setup(r => r.SalvarAlteracoesAsync()).Returns(Task.CompletedTask);

			var emailException = new Exception("SMTP down");
			_emailMock
				.Setup(e => e.EnviarCancelamentoReservaAsync(
					It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
				.ThrowsAsync(emailException);

			var request = new EditarReservaRequest
			{
				Id = id,
				Sala = "Sala Y",
				Usuario = "y@ex.com",
				DataHoraReserva = DateTime.UtcNow.AddDays(2)
			};

			// Act
			var result = await _handler.HandleAsync(request);

			// Assert
			Assert.Equal(id, result.Id);
			Assert.False(result.EmailEnviado);
			Assert.Equal(emailException.Message, result.Erro);

			_emailMock.Verify(e => e.EnviarCancelamentoReservaAsync(
				request.Usuario, request.Sala, It.IsAny<DateTime>()), Times.Once);
		}

		[Fact]
		public async Task HandleAsync_Deve_Lancar_Se_ReservaNaoEncontrada()
		{
			// Arrange
			var id = Guid.NewGuid();
			_repoMock.Setup(r => r.ObterPorIdAsync(id))
					 .ReturnsAsync((Reserva?)null);

			var request = new EditarReservaRequest { Id = id };

			// Act & Assert
			var ex = await Assert.ThrowsAsync<InvalidOperationException>(
				() => _handler.HandleAsync(request));
			Assert.Equal("Reserva não encontrada.", ex.Message);
		}

		[Fact]
		public async Task HandleAsync_Deve_Lancar_Se_StatusNaoConfirmada()
		{
			// Arrange
			var id = Guid.NewGuid();
			var reserva = new Reserva
			{
				Id = id,
				Sala = "Sala A",
				Usuario = "u@e.com",
				DataHoraReserva = DateTime.UtcNow.AddDays(1),
				Status = StatusReserva.Cancelada
			};
			_repoMock.Setup(r => r.ObterPorIdAsync(id))
					 .ReturnsAsync(reserva);

			var request = new EditarReservaRequest
			{
				Id = id,
				Sala = reserva.Sala,
				Usuario = reserva.Usuario,
				DataHoraReserva = reserva.DataHoraReserva
			};

			// Act & Assert
			var ex = await Assert.ThrowsAsync<InvalidOperationException>(
				() => _handler.HandleAsync(request));
			Assert.Equal("Apenas reservas confirmadas podem ser editadas.", ex.Message);
		}

		[Fact]
		public async Task HandleAsync_Deve_Lancar_Se_ConflitoDeHorario()
		{
			// Arrange
			var id = Guid.NewGuid();
			var reserva = new Reserva
			{
				Id = id,
				Sala = "Sala Z",
				Usuario = "z@ex.com",
				DataHoraReserva = DateTime.UtcNow.AddDays(1),
				Status = StatusReserva.Confirmada
			};
			_repoMock.Setup(r => r.ObterPorIdAsync(id))
					 .ReturnsAsync(reserva);
			// Conflito: devolve uma lista com reserva diferente
			_repoMock.Setup(r => r.ObterPorSalaEPeriodoAsync(
					It.IsAny<string>(), It.IsAny<DateTime>()))
					 .ReturnsAsync(new List<Reserva> { new Reserva { Id = Guid.NewGuid() } });

			var request = new EditarReservaRequest
			{
				Id = id,
				Sala = reserva.Sala,
				Usuario = reserva.Usuario,
				DataHoraReserva = DateTime.UtcNow.AddDays(2)
			};

			// Act & Assert
			var ex = await Assert.ThrowsAsync<InvalidOperationException>(
				() => _handler.HandleAsync(request));
			Assert.Equal("Já existe uma reserva para essa sala nesse horário.", ex.Message);
		}
	}
}
