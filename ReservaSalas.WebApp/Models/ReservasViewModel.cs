using ReservaSalas.Application.DTOs;

namespace ReservaSalas.WebApp.Models
{
	public class ReservasViewModel
	{
		public string? Sala { get; set; }
		public string? Usuario { get; set; }
		public PagedResult<ReservaResponse> Resultado { get; set; } = new();
	}
}
