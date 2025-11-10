using Api.Factories.Implementations;
using Api.Factories.Interfaces;
using Api.Repositories.Implementations;
using Api.Repositories.Interfaces;
using Api.Services.Implementations;
using Api.Services.Interfaces;
using Api.Strategies.Implementations;
using Api.Strategies.Interfaces;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Employee Benefit Cost Calculation Api",
        Description = "Api to support employee benefit cost calculations"
    });
});

var allowLocalhost = "allow localhost";
builder.Services.AddCors(options =>
{
    options.AddPolicy(allowLocalhost,
        policy => { policy.WithOrigins("http://localhost:3000", "http://localhost"); });
});

builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IEmployeeRepository, MockEmployeeRepository>();
builder.Services.AddScoped<IDependentRepository, MockDependentRepository>();
builder.Services.AddScoped<IPaycheckConfigurationRepository, MockPaycheckConfigurationRepository>();
builder.Services.AddScoped<IEmployeePaycheckService, EmployeePaycheckService>();

// register strategies
builder.Services.AddScoped<IDeductionStrategy, BaseBenefitsDeductionStrategy>();
builder.Services.AddScoped<IDeductionStrategy, DependentsDeductionStrategy>();
builder.Services.AddScoped<IDeductionStrategy, HighWageEarnerDeductionStrategy>();
builder.Services.AddScoped<IPaycheckCalculationStrategy, USPaycheckCalculationStrategy>();
builder.Services.AddScoped<IPaycheckCalculationStrategyFactory, PaycheckCalculationStrategyFactory>();
builder.Services.AddScoped<IDeductionStrategyFactory, DeductionStrategyFactory>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(allowLocalhost);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
