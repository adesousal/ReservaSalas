using ReservaSalas.Domain.Enums;

namespace ReservaSalas.Application.DTOs
{
	public class ReservaResponse
	{
		public Guid Id { get; set; }
		public string Sala { get; set; } = string.Empty;
		public string Usuario { get; set; } = string.Empty;
		public DateTime DataHoraReserva { get; set; }
		public StatusReserva Status { get; set; }
	}
}
