using System.ComponentModel.DataAnnotations;

namespace ReservaSalas.WebApp.Models
{
	public class ReservaRequest
	{
		public Guid Id { get; set; }

		[Display(Name = "Sala")]
		[Required(ErrorMessage = "A sala é obrigatória.")]
		public string Sala { get; set; } = string.Empty;

		[Display(Name = "Usuário (e-mail)")]
		[Required(ErrorMessage = "O usuário (e-mail) é obrigatório.")]
		[EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
		public string Usuario { get; set; } = string.Empty;

		[Required(ErrorMessage = "A data e hora são obrigatórias.")]
		[Display(Name = "Data e Hora")]
		public DateTime DataHoraReserva { get; set; }

		[Display(Name = "Status")]
		public string Status { get; set; } = "Confirmada";
	}
}
