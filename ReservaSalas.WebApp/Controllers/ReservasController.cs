using Microsoft.AspNetCore.Mvc;
using ReservaSalas.Application.DTOs;
using ReservaSalas.WebApp.Models;
using ReservaSalas.WebApp.Services;

namespace ReservaSalas.WebApp.Controllers
{
	public class ReservasController : Controller
	{
		private readonly IReservaApiService _api;

		public ReservasController(IReservaApiService api)
		{
			_api = api;
		}

		public async Task<IActionResult> Index(string? sala, string? usuario, DateTime? dataInicio, DateTime? dataFim, int page = 1)
		{
			var filtro = new ReservasFilter
			{
				Sala = sala?.Trim(),
				Usuario = usuario?.Trim(),
				DataInicio = dataInicio,
				DataFim = dataFim,
				PageNumber = page,
				PageSize = 10
			};

			var resultado = await _api.ListarAsync(filtro);

			var model = new ReservasViewModel
			{
				Sala = sala,
				Usuario = usuario,
				Resultado = resultado
			};

			return View(model);
		}

		// GET: Reservas/Create
		[HttpGet]
		public IActionResult Create()
		{
			var model = new ReservaRequest();

			model.DataHoraReserva = DateTime.Now;

			return View(model);
		}

		// POST: Reservas/Create
		[HttpPost]
		public async Task<IActionResult> Create(ReservaRequest request)
		{
			if (!ModelState.IsValid)
			{
				TempData["Erro"] = "Por favor, corrija os erros do formulário.";
				return View(request);
			}

			bool criada = false;

			try
			{
				var reservaRequest = new CriarReservaRequest()
				{
					Sala = request.Sala,
					Usuario= request.Usuario,
					DataHoraReserva = request.DataHoraReserva
				};

				await _api.CriarAsync(reservaRequest);

				TempData["Mensagem"] = "Reserva criada com sucesso.";

				return RedirectToAction(nameof(Index));
			}
			catch (HttpRequestException ex)
			{
				TempData["Erro"] = "Erro ao conectar com a API.";
			}
			catch (Exception ex)
			{
				if (ex.Message.ToLower().Contains("reserva criada"))
					criada = true;

				TempData["Erro"] = ex.Message;				
			}

			if(criada)
				return RedirectToAction(nameof(Index));

			return View(request);
		}

		[HttpGet]
		public async Task<IActionResult> Edit(Guid id)
		{
			try
			{
				var reserva = await _api.ObterPorIdAsync(id);

				if (reserva == null)
				{
					TempData["Erro"] = "Reserva não encontrada.";
					return View();
				}					

				// Mapeia ReservaResponse para ReservaRequest para preencher o formulário
				var model = new ReservaRequest
				{
					Sala = reserva.Sala,
					Usuario = reserva.Usuario,
					DataHoraReserva = reserva.DataHoraReserva,
					Status = reserva.Status.ToString()
				};

				return View(model);
			}
			catch
			{
				TempData["Erro"] = "Erro ao buscar reserva.";
				return View();
			}
		}

		// POST: Reservas/Edit/5
		[HttpPost]
		public async Task<IActionResult> Edit(Guid id, ReservaRequest request)
		{
			if (!ModelState.IsValid)
			{
				TempData["Erro"] = "Por favor, corrija os erros do formulário.";
				return View(request);
			}

			bool editada = false;

			try
			{
				var reservaRequest = new EditarReservaRequest()
				{
					Id = id,
					Sala = request.Sala,
					Usuario = request.Usuario,
					DataHoraReserva = request.DataHoraReserva
				};

				await _api.EditarAsync(reservaRequest);

				TempData["Mensagem"] = "Reserva alterada com sucesso.";

				return RedirectToAction(nameof(Index));
			}
			catch (HttpRequestException)
			{
				TempData["erro"] = "Erro ao conectar com a API.";				
			}
			catch (Exception ex)
			{
				if (ex.Message.ToLower().Contains("reserva alterada"))
					editada = true;

				TempData["erro"] = ex.Message;
			}

			if (editada)
				return RedirectToAction(nameof(Index));

			return View(request);
		}

		[HttpGet]
		public async Task<IActionResult> Cancelar(Guid id)
		{
			try
			{
				await _api.CancelarAsync(id);
				TempData["Mensagem"] = "Reserva cancelada com sucesso.";
			}
			catch (HttpRequestException)
			{
				TempData["Erro"] = "Erro ao conectar com a API.";
			}
			catch (Exception ex)
			{
				TempData["Erro"] = ex.Message;
			}

			return RedirectToAction(nameof(Index));
		}
	}
}
