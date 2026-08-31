using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Sad.Api.Auth;
using Sad.Api.Data;
using Sad.Api.Services.Auth;
using Sad.Api.Services.Catalog;
using Sad.Api.Services.Dashboard;
using Sad.Api.Services.Sales;
using SADWebApi.Services.Sales;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Controllers / Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo
  {
    Title = "SADWebApi",
    Version = "v1"
  });

  c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    Scheme = "bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "Paste your JWT token here. Do NOT write Bearer, only the token."
  });

  c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
      Array.Empty<string>()
    }
  });
});

// Database
// Production: set ConnectionStrings__SadDb in Render with the Neon PostgreSQL connection string.
// Local development / EF tooling: use .NET User Secrets for ConnectionStrings:SadDb.
var connectionString = builder.Configuration.GetConnectionString("SadDb");

if (string.IsNullOrWhiteSpace(connectionString))
{
  throw new InvalidOperationException(
    "Missing database connection string. Configure ConnectionStrings:SadDb using .NET User Secrets or ConnectionStrings__SadDb.");
}

builder.Services.AddDbContext<SadDbContext>(opt =>
  opt.UseNpgsql(connectionString));

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<ICreditCardApplicationsService, CreditCardApplicationsService>();
builder.Services.AddScoped<IMembershipSalesService, MembershipSalesService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddScoped<IStoreService, StoreService>();
builder.Services.AddScoped<IUserDailySettingsService, UserDailySettingsService>();

builder.Services.AddHttpClient();

// CORS
builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowAngularApp", policy =>
  {
    policy
      .WithOrigins(
        "http://localhost:4200",
        "http://localhost:4300",
        "https://sad.thekiddycloud.com",
        "https://sad.fidelfm.com"
      )
      .AllowAnyHeader()
      .AllowAnyMethod();
  });
});

// Options
builder.Services.Configure<JwtOptions>(
  builder.Configuration.GetSection("Jwt")
);

builder.Services.Configure<MicrosoftOAuthOptions>(
  builder.Configuration.GetSection("Auth:Microsoft")
);

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// JWT config validation
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>();

if (jwt is null ||
    string.IsNullOrWhiteSpace(jwt.Issuer) ||
    string.IsNullOrWhiteSpace(jwt.Audience) ||
    string.IsNullOrWhiteSpace(jwt.SigningKey))
{
  throw new InvalidOperationException(
    "Missing JWT configuration. Check Jwt__Issuer, Jwt__Audience and Jwt__SigningKey.");
}

// Microsoft OAuth config validation
var microsoftOAuth = builder.Configuration
  .GetSection("Auth:Microsoft")
  .Get<MicrosoftOAuthOptions>();

if (microsoftOAuth is null ||
    string.IsNullOrWhiteSpace(microsoftOAuth.TenantId) ||
    string.IsNullOrWhiteSpace(microsoftOAuth.ClientId) ||
    string.IsNullOrWhiteSpace(microsoftOAuth.ClientSecret) ||
    string.IsNullOrWhiteSpace(microsoftOAuth.RedirectUri) ||
    string.IsNullOrWhiteSpace(microsoftOAuth.FrontendLoginUrl) ||
    string.IsNullOrWhiteSpace(microsoftOAuth.FrontendSuccessUrl))
{
  throw new InvalidOperationException(
    "Missing Microsoft OAuth configuration. Check Auth__Microsoft__TenantId, Auth__Microsoft__ClientId, Auth__Microsoft__ClientSecret, Auth__Microsoft__RedirectUri, Auth__Microsoft__FrontendLoginUrl and Auth__Microsoft__FrontendSuccessUrl.");
}

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(options =>
  {
    options.TokenValidationParameters = new TokenValidationParameters
    {
      ValidateIssuer = true,
      ValidateAudience = true,
      ValidateLifetime = true,
      ValidateIssuerSigningKey = true,

      ValidIssuer = jwt.Issuer,
      ValidAudience = jwt.Audience,
      IssuerSigningKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(jwt.SigningKey)
      ),

      ClockSkew = TimeSpan.FromMinutes(1)
    };
  });

builder.Services.AddAuthorization();

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Pipeline
app.UseHttpsRedirection();

app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
