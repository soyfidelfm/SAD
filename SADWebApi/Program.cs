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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<SadDbContext>(opt =>
	opt.UseSqlServer(builder.Configuration.GetConnectionString("SadDb")));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<ICreditCardApplicationsService, CreditCardApplicationsService>();
builder.Services.AddScoped<IMembershipSalesService, MembershipSalesService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddScoped<IStoreService, StoreService>();
builder.Services.AddScoped<IUserDailySettingsService, UserDailySettingsService>();

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

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(o =>
	{
		o.TokenValidationParameters = new TokenValidationParameters
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

builder.Services.AddHttpClient();
builder.Services.Configure<MicrosoftOAuthOptions>(
	builder.Configuration.GetSection("Auth:Microsoft")
);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();