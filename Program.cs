using DSW_3P_PR02.Filters;
using DSW_3P_PR02.Repositories;
using DSW_3P_PR02.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options => options.Filters.Add<BusinessExceptionFilter>());
builder.Services.AddOpenApi();

// Controller -> IService -> IRepository -> JSON
builder.Services.AddSingleton<IPersonajeRepository, PersonajeJsonRepository>();
builder.Services.AddScoped<IPersonajeService, PersonajeService>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.Run();
