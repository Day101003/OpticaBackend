using ProyectoFinal.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ProyectoFinal.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// 👇 INTERNACIONALIZACIÓN
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "en", "es" };
    options.SetDefaultCulture("es")
           .AddSupportedCultures(supportedCultures)
           .AddSupportedUICultures(supportedCultures);
});

// 👇 Controladores con opciones de serialización
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// 👇 Autenticación JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]))
        };
    });

// 👇 Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.WithOrigins("http://localhost:5173")
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

// 👇 Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 👇 DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AppDbContext")));

// 👇 Servicios propios
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// 👇 Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProyectoFinal API V1");
        c.RoutePrefix = "swagger";
    });
}

// 👇 Usa archivos estáticos si corresponde
app.UseStaticFiles();

// 👇 Internacionalización en el pipeline
var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);

app.UseCors("AllowAllOrigins");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
