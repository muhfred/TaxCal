using TaxCal.Application.Interfaces;
using TaxCal.Application.Services;
using TaxCal.Infrastructure.InMemory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<ITaxRuleRepository, InMemoryTaxRuleRepository>();
builder.Services.AddScoped<IConfigureTaxRuleService, ConfigureTaxRuleService>();
builder.Services.AddScoped<ICalculateTaxService, CalculateTaxService>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Expose entry point for integration tests (WebApplicationFactory<Program>)
public partial class Program { }
