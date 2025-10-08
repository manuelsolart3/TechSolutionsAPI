using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TechSolutionsAPI.Services;
using TechSolutionsAPI.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------
//  DbContext
// ------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));


// ------------------------------------------------------
// CORS
// ------------------------------------------------------
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


// ------------------------------------------------------
//  Autenticación JWT
// ------------------------------------------------------
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

builder.Services.AddAuthentication(options =>
{
    //  JWT Bearer
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // validar los tokens JWT
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,             
        ValidateAudience = true,           
        ValidateLifetime = true,           
        ValidateIssuerSigningKey = true,   

        ValidIssuer = issuer,              
        ValidAudience = audience,          

        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),

        ClockSkew = TimeSpan.Zero          
    };
});


// ------------------------------------------------------
//  Authorization
// ------------------------------------------------------
builder.Services.AddAuthorization();

// ------------------------------------------------------
//  Swagger
// ------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TechSolutions API",
        Version = "v1"
    });

    // autenticación JWT en Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Ingrese 'Bearer' seguido de su token JWT.",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IServiceManagementService, ServiceManagementService>();


// ------------------------------------------------------
//  controladores
// ------------------------------------------------------
builder.Services.AddControllers();


// ------------------------------------------------------
//  pipeline HTTP
// ------------------------------------------------------
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
