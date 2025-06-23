using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using ReservaSalas.Domain.Interfaces;

namespace ReservaSalas.Infrastructure.Services
{
	public class EmailService : IEmailService
	{
		private readonly IConfiguration _configuration;

		public EmailService(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task EnviarEmailAsync(string destinatario, string assunto, string corpoHtml)
		{
			var message = new MimeMessage();
			message.From.Add(new MailboxAddress("Sistema de Reservas", _configuration["Email:Remetente"]));
			message.To.Add(new MailboxAddress("", destinatario));
			message.Subject = assunto;

			var bodyBuilder = new BodyBuilder
			{
				HtmlBody = corpoHtml
			};
			message.Body = bodyBuilder.ToMessageBody();

			using var smtp = new SmtpClient();
			await smtp.ConnectAsync(_configuration["Email:Servidor"], int.Parse(_configuration["Email:Porta"]), SecureSocketOptions.StartTls);
			await smtp.AuthenticateAsync(_configuration["Email:Usuario"], _configuration["Email:Senha"]);
			await smtp.SendAsync(message);
			await smtp.DisconnectAsync(true);
		}

		public async Task EnviarConfirmacaoReservaAsync(string destinatario, string sala, DateTime dataHora)
		{
			var assunto = "Reserva Confirmada";
			var corpo = $"Sua reserva para a sala {sala} foi confirmada para {dataHora:dd/MM/yyyy HH:mm}.";
			await EnviarEmailAsync(destinatario, assunto, corpo);
		}

		public async Task EnviarCancelamentoReservaAsync(string destinatario, string sala, DateTime dataHora)
		{
			var assunto = "Reserva Cancelada";
			var corpo = $"Sua reserva para a sala {sala} em {dataHora:dd/MM/yyyy HH:mm} foi cancelada.";
			await EnviarEmailAsync(destinatario, assunto, corpo);
		}
	}
}
