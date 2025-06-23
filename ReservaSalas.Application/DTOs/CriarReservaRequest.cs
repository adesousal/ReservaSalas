using System.ComponentModel.DataAnnotations;

namespace ReservaSalas.Application.DTOs
{
	public class CriarReservaRequest
	{
		[Required(ErrorMessage = "A sala é obrigatória.")]
		public string Sala { get; set; } = string.Empty;

		[Required(ErrorMessage = "O usuário (e-mail) é obrigatório.")]
		[EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
		public string Usuario { get; set; } = string.Empty;

		[Required(ErrorMessage = "A data e hora são obrigatórias.")]
		public DateTime DataHoraReserva { get; set; }
	}
}
