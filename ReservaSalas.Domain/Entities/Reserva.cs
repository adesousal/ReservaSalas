using ReservaSalas.Domain.Enums;

namespace ReservaSalas.Domain.Entities
{
	public class Reserva
	{
		public Guid Id { get; set; }
		public string Sala { get; set; } = string.Empty;
		public string Usuario { get; set; } = string.Empty;
		public DateTime DataHoraReserva { get; set; }
		public StatusReserva Status { get; set; }

		public bool PodeSerCancelada()
		{
			return DataHoraReserva > DateTime.Now.AddHours(24) && Status == StatusReserva.Confirmada;
		}
	}
}
