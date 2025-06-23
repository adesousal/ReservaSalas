using Moq;
using ReservaSalas.Application.DTOs;
using ReservaSalas.Application.UseCases;
using ReservaSalas.Domain.Entities;
using ReservaSalas.Domain.Enums;
using ReservaSalas.Domain.Filters;
using ReservaSalas.Domain.Repositories;

namespace ReservaSalas.Tests.Application.UseCases
{
	public class ListarReservasHandlerTests
	{
		private readonly Mock<IReservaRepository> _repoMock = new();
		private readonly ListarReservasHandler _handler;

		public ListarReservasHandlerTests()
		{
			_handler = new ListarReservasHandler(_repoMock.Object);
		}

		[Fact]
		public async Task HandleAsync_Deve_Retornar_PagedResult_Com_Itens_E_Metadados()
		{
			// Arrange
			var id1 = Guid.NewGuid();
			var agora = DateTime.UtcNow;
			var reservasDomain = new List<Reserva>
			{
				new Reserva { Id = id1, Sala = "Sala X", Usuario = "u@e.com", DataHoraReserva = agora, Status = StatusReserva.Confirmada }
			};
			_repoMock
				.Setup(r => r.ObterReservasFiltradasAsync(It.IsAny<ReservaFilter>()))
				.ReturnsAsync((reservasDomain, 1));

			var filtroApp = new ReservasFilter
			{
				Sala = "Sala X",
				Usuario = "u@e.com",
				DataInicio = null,
				DataFim = null,
				PageNumber = 2,
				PageSize = 5
			};

			// Act
			var result = await _handler.HandleAsync(filtroApp);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(1, result.TotalCount);
			Assert.Equal(2, result.PageNumber);
			Assert.Equal(5, result.PageSize);

			var item = Assert.Single(result.Items);
			Assert.Equal(id1, item.Id);
			Assert.Equal("Sala X", item.Sala);
			Assert.Equal("u@e.com", item.Usuario);
			Assert.Equal(agora, item.DataHoraReserva);
			Assert.Equal(StatusReserva.Confirmada, item.Status);
		}

		[Fact]
		public async Task HandleAsync_Deve_Chamar_Repositorio_Com_Filtro_Mapeado()
		{
			// Arrange
			ReservasFilter filtroApp = new()
			{
				Sala = "A",
				Usuario = "B",
				DataInicio = DateTime.Today,
				DataFim = DateTime.Today.AddDays(1),
				PageNumber = 3,
				PageSize = 7
			};
			ReservaFilter filtroEsperado = null!;

			_repoMock
			  .Setup(r => r.ObterReservasFiltradasAsync(It.IsAny<ReservaFilter>()))
			  .Callback<ReservaFilter>(f => filtroEsperado = f)
			  .ReturnsAsync((new List<Reserva>(), 0));

			// Act
			await _handler.HandleAsync(filtroApp);

			// Assert: o repositório deve ser chamado com filtro convertido
			Assert.NotNull(filtroEsperado);
			Assert.Equal(filtroApp.Sala, filtroEsperado.Sala);
			Assert.Equal(filtroApp.Usuario, filtroEsperado.Usuario);
			Assert.Equal(filtroApp.DataInicio, filtroEsperado.DataInicio);
			Assert.Equal(filtroApp.DataFim, filtroEsperado.DataFim);
			Assert.Equal(filtroApp.PageNumber, filtroEsperado.PageNumber);
			Assert.Equal(filtroApp.PageSize, filtroEsperado.PageSize);
		}

		[Fact]
		public async Task ObterPorIdAsync_Deve_Retornar_Reserva_Do_Repositorio()
		{
			// Arrange
			var id = Guid.NewGuid();
			var reservaDomain = new Reserva
			{
				Id = id,
				Sala = "Sala Y",
				Usuario = "x@y.com",
				DataHoraReserva = DateTime.UtcNow.AddHours(2),
				Status = StatusReserva.Confirmada
			};
			_repoMock
				.Setup(r => r.ObterPorIdAsync(id))
				.ReturnsAsync(reservaDomain);

			// Act
			var result = await _handler.ObterPorIdAsync(id);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(reservaDomain, result);
		}
	}
}
