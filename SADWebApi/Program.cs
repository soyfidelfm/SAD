using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
builder.Services.AddSwaggerGen();

// Database
var connectionString = builder.Configuration.GetConnectionString("SadDb");

if (string.IsNullOrWhiteSpace(connectionString))
{
  throw new InvalidOperationException("Missing connection string: ConnectionStrings__SadDb");
}

builder.Services.AddDbContext<SadDbContext>(opt =>
  opt.UseSqlServer(connectionString));

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
        "https://sad.thekiddycloud.com"
      )
      .AllowAnyHeader()
      .AllowAnyMethod();
  });
});

// Options
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
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
    "Missing JWT configuration. Check Jwt__Issuer, Jwt__Audience, Jwt__SigningKey in Render."
  );
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
