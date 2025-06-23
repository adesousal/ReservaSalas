using ReservaSalas.WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient<IReservaApiService, ReservaApiService>(client =>
{
	client.BaseAddress = new Uri("https://localhost:5001/"); // ou URL da sua API
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Reservas}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
