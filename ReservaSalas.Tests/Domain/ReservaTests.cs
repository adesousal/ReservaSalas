using ReservaSalas.Domain.Entities;
using ReservaSalas.Domain.Enums;

namespace ReservaSalas.Tests;

public class ReservaTests
{
	[Fact]
	public void Deve_Retornar_Verdadeiro_Se_Faltar_Mais_De_24h()
	{
		var reserva = new Reserva
		{
			DataHoraReserva = DateTime.Now.AddDays(2),
			Status = StatusReserva.Confirmada
		};

		var resultado = reserva.PodeSerCancelada();

		Assert.True(resultado);
	}

	[Fact]
	public void Deve_Retornar_Falso_Se_Faltar_Menos_De_24h()
	{
		var reserva = new Reserva
		{
			DataHoraReserva = DateTime.Now.AddHours(10),
			Status = StatusReserva.Confirmada
		};

		var resultado = reserva.PodeSerCancelada();

		Assert.False(resultado);
	}

	[Fact]
	public void Deve_Retornar_Falso_Se_Status_Nao_For_Confirmada()
	{
		var reserva = new Reserva
		{
			DataHoraReserva = DateTime.Now.AddDays(2),
			Status = StatusReserva.Cancelada
		};

		var resultado = reserva.PodeSerCancelada();

		Assert.False(resultado);
	}
}
