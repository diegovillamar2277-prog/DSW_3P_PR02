using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using DSW_3P_PR02.Data;
using DSW_3P_PR02.Filters;
using DSW_3P_PR02.Repositories;
using DSW_3P_PR02.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options => options.Filters.Add<BusinessExceptionFilter>());
builder.Services.AddOpenApi();

// Controller -> IService -> IRepository -> SQL Server (EF Core)
builder.Services.AddScoped<IPersonajeRepository, PersonajeSqlRepository>();
builder.Services.AddScoped<IPersonajeService, PersonajeService>();
builder.Services.AddProblemDetails();

// Configuracion del SQL SERVER (cadena en user-secrets, no en el repositorio)
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("SqlServerConnection")
    )
);

// Configuracion del JWT (la clave Jwt:Key vive en user-secrets, no en el repositorio)
var jwt = builder.Configuration.GetSection("Jwt");
var key = Convert.FromBase64String(
   jwt["Key"] ?? throw new InvalidOperationException(
       "Falta configurar Jwt:Key"));
builder.Services
   .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
   .AddJwtBearer(options =>
   {
       options.TokenValidationParameters =
           new TokenValidationParameters
           {
               ValidateIssuer = true,
               ValidIssuer = jwt["Issuer"],
               ValidateAudience = true,
               ValidAudience = jwt["Audience"],
               ValidateLifetime = true,
               ClockSkew = TimeSpan.Zero,
               ValidateIssuerSigningKey = true,
               IssuerSigningKey =
                   new SymmetricSecurityKey(key)
           };
   });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
