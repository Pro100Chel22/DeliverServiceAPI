using ASPDotNetWebAPI.Middlewares;
using ASPDotNetWebAPI.Models;
using ASPDotNetWebAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Modification of Swagger for authentication
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string> ()
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

// Configuring Authentication
var secret = builder.Configuration["JWTTokenSettings:Secret"] ?? throw new InvalidOperationException("Secret not configured");
var ValidIssuer = builder.Configuration["JWTTokenSettings:ValidIssuer"] ?? throw new InvalidOperationException("ValidIssuer not configured");
var TimeSpanSecond = builder.Configuration.GetValue<int>("JWTTokenSettings:TimeSpanSecond");
builder.Services.AddAuthentication(authOptions =>
{
    authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(jwtOptions =>
    {
        jwtOptions.RequireHttpsMetadata = false; // Develop 
        jwtOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)),
            ValidateIssuer = true,
            ValidIssuer = ValidIssuer,
            ValidateAudience = false,
            ClockSkew = new TimeSpan(0, 0, TimeSpanSecond)
        };

    });

// Connecting the database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

// Adding services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IDishService, DishService>();
builder.Services.AddScoped<IBasketService, BasketService>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddHostedService<CleanRefreshTokensDbHostedService>();

// Cleaning of logger providers
builder.Logging.ClearProviders();
builder.Host.UseNLog();

var app = builder.Build();

// Creating auto migrations
using var serviceScope = app.Services.CreateScope();
var dbContext = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
dbContext?.Database.Migrate();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
