using ImageMaster.Data.Contexts;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ImagesContext>(opt
	=> opt.UseSqlServer(builder.Configuration.GetConnectionString("ImagesDB")));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer()
	.AddHttpClient()
	.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();