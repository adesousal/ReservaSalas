using ReservaSalas.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddApplication();

builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new() { Title = "Reserva Salas API", Version = "v1" });
});

var app = builder.Build();

app.UseCors("AllowMvcApp");
app.UseSwagger();

app.UseSwaggerUI(c =>
{
	c.SwaggerEndpoint("/swagger/v1/swagger.json", "Reserva Salas API V1");
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

