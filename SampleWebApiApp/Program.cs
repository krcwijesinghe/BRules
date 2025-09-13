using SampleWebApiApp.BusinessLogic;
using SampleWebApiApp.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<IItemsRepository, ItemsRepository>();
builder.Services.AddScoped<IRuleSetRepository, RuleSetRepository>();
builder.Services.AddScoped<IItemsManager, ItemsManager>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
