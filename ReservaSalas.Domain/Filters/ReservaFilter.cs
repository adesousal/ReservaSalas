namespace ReservaSalas.Domain.Filters
{
	public class ReservaFilter
	{
		public string? Sala { get; set; }
		public string? Usuario { get; set; }
		public DateTime? DataInicio { get; set; }
		public DateTime? DataFim { get; set; }
		public int PageNumber { get; set; } = 1;
		public int PageSize { get; set; } = 10;
	}
}
