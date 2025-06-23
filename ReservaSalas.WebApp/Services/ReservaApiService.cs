using ReservaSalas.Application.DTOs;

namespace ReservaSalas.WebApp.Services
{
	public class ReservaApiService : IReservaApiService
	{
		private readonly HttpClient _http;

		public ReservaApiService(HttpClient http)
		{
			_http = http;
		}

		public async Task<PagedResult<ReservaResponse>> ListarAsync(ReservasFilter filtro)
		{
			var query = $"?Sala={filtro.Sala}&Usuario={filtro.Usuario}&DataInicio={filtro.DataInicio}&DataFim={filtro.DataFim}&PageNumber={filtro.PageNumber}&PageSize={filtro.PageSize}";
			return await _http.GetFromJsonAsync<PagedResult<ReservaResponse>>($"api/reservas{query}");
		}

		public async Task<ReservaResponse> ObterPorIdAsync(Guid id)
			=> await _http.GetFromJsonAsync<ReservaResponse>($"api/reservas/{id}");

		public async Task CriarAsync(CriarReservaRequest request)
		{
			var response = await _http.PostAsJsonAsync("api/reservas", request);

			if (!response.IsSuccessStatusCode)
			{
				// Lê o corpo da resposta com erro para exibir no MVC
				var content = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
				var message = content != null && content.ContainsKey("message")
					? content["message"]
					: "Erro ao criar reserva.";

				throw new ApplicationException(message);
			}
		}

		public async Task EditarAsync(EditarReservaRequest request)
		{
			var response = await _http.PutAsJsonAsync($"api/reservas/{request.Id}", request);

			if (!response.IsSuccessStatusCode)
			{
				// Lê o corpo da resposta com erro para exibir no MVC
				var content = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
				var message = content != null && content.ContainsKey("message")
					? content["message"]
					: "Erro ao alterar reserva.";

				throw new ApplicationException(message);
			}
		}

		public async Task CancelarAsync(Guid id)
		{
			var response = await _http.DeleteAsync($"api/reservas/{id}");

			if (!response.IsSuccessStatusCode)
			{
				// Lê o corpo da resposta com erro para exibir no MVC
				var content = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
				var message = content != null && content.ContainsKey("message")
					? content["message"]
					: "Erro ao cancelar reserva.";

				throw new ApplicationException(message);
			}
		}
	}
}
