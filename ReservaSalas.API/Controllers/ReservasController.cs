using Microsoft.AspNetCore.Mvc;
using ReservaSalas.Application.DTOs;
using ReservaSalas.Application.UseCases;

namespace ReservaSalas.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ReservasController : ControllerBase
	{
		private readonly CriarReservaHandler _criarReservaHandler;
		private readonly EditarReservaHandler _editarReservaHandler;
		private readonly CancelarReservaHandler _cancelarReservaHandler;
		private readonly ListarReservasHandler _listarReservasHandler;

		public ReservasController(CriarReservaHandler criarReservaHandler,
								  EditarReservaHandler editarReservaHandler,
								  CancelarReservaHandler cancelarReservaHandler,
								  ListarReservasHandler listarReservasHandler)
		{
			_criarReservaHandler = criarReservaHandler;
			_editarReservaHandler = editarReservaHandler;
			_cancelarReservaHandler = cancelarReservaHandler;
			_listarReservasHandler = listarReservasHandler;
		}

		[HttpPost]
		public async Task<IActionResult> CriarReserva([FromBody] CriarReservaRequest request)
		{
			try
			{
				var result = await _criarReservaHandler.HandleAsync(request);

				if (!result.EmailEnviado)
					throw new Exception($"Reserva criada, mas falha ao enviar e-mail.\n\n {result.Erro}");

				return CreatedAtAction(nameof(ObterPorIdAsync), new { result.Id }, new { result.Id });
			}
			catch (Exception ex)
			{
				return Conflict(new { message = ex.Message });
			}
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> EditarReserva(Guid id, [FromBody] EditarReservaRequest request)
		{
			if (id != request.Id)
				return BadRequest(new { message = "ID da URL não corresponde ao corpo da requisição." });

			try
			{
				var result = await _editarReservaHandler.HandleAsync(request);

				if (!result.EmailEnviado)
					throw new Exception($"Reserva alterada, mas falha ao enviar e-mail.\n\n {result.Erro}");

				return NoContent();
			}
			catch (InvalidOperationException ex)
			{
				return Conflict(new { message = ex.Message });
			}
			catch (Exception ex)
			{
				return Conflict(new { message = ex.Message });
			}
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> ObterPorIdAsync(Guid id)
		{
			var reserva = await _listarReservasHandler.ObterPorIdAsync(id);
			if (reserva is null)
				return NotFound();

			var reservaDto = new ReservaResponse
			{
				Id = reserva.Id,
				Sala = reserva.Sala,
				Usuario = reserva.Usuario,
				DataHoraReserva = reserva.DataHoraReserva,
				Status = reserva.Status
			};

			return Ok(reservaDto);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> CancelarReserva(Guid id)
		{
			try
			{
				var result = await _cancelarReservaHandler.HandleAsync(id);

				if (!result.EmailEnviado)
					throw new Exception($"Reserva cancelada, mas falha ao enviar e-mail.\n\n {result.Erro}");

				return NoContent();
			}
			catch (Exception ex)
			{
				return Conflict(new { message = ex.Message });
			}
		}

		[HttpGet]
		public async Task<IActionResult> ObterTodas([FromQuery] ReservasFilter filtro)
		{
			var resultado = await _listarReservasHandler.HandleAsync(filtro);
			return Ok(resultado);
		}
	}
}
