using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using DSW_3P_PR02.Data;
using DSW_3P_PR02.Filters;
using DSW_3P_PR02.Repositories;
using DSW_3P_PR02.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options => options.Filters.Add<BusinessExceptionFilter>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Boton Authorize en Swagger UI para probar con el token JWT
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Pega el access_token obtenido en /api/auth/login"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

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

// CORS: los origenes permitidos (cliente de evaluacion del docente) se leen de Cors:Origins.
// UseCors debe ir antes de autenticacion/autorizacion para que el preflight (OPTIONS, sin token)
// no sea rechazado con 401.
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [])
    .AllowAnyHeader()   // permite Content-Type y Authorization (el token Bearer)
    .AllowAnyMethod())); // permite GET, POST, PUT, DELETE y OPTIONS

var app = builder.Build();

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
