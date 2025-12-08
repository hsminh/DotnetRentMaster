using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using RentMaster.Data;
using RentMaster.Accounts;
using RentMaster.Core.Cloudinary;
using RentMaster.Core.Middleware;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Npgsql;
using RentMaster.Addresses;
using RentMaster.Addresses.Commands;
using RentMaster.Addresses.Services;
using RentMaster.Ai;
using RentMaster.Core.Backend.Auth;
using RentMaster.Management.RealEstate;
using RentMaster.Management.RealEstate.Validators;
using RentMaster.Management.Tenant;
using RentMaster.Management.ConsumerContact;
using RentMaster.partner.Firebase.Services;
using RentMaster.partner.Firebase.Services.Client;
// using Payments.VnPay;
using IUserChannelNotificationService = RentMaster.partner.Firebase.Services.Interfaces.IUserChannelNotificationService;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------
// Load environment variables from .env
// ------------------------------
var root = Directory.GetCurrentDirectory();
Env.Load(Path.Combine(root, ".env"));

builder.Configuration["Jwt:Key"] = Environment.GetEnvironmentVariable("JWT_KEY");
builder.Configuration["Jwt:Issuer"] = Environment.GetEnvironmentVariable("JWT_ISSUER");
builder.Configuration["Jwt:Audience"] = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

// ------------------------------
// Build PostgreSQL connection string
// ------------------------------
var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
var dbName = Environment.GetEnvironmentVariable("DB_NAME");
var dbUser = Environment.GetEnvironmentVariable("DB_USER");
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";


// ------------------------------
// Register DbContext
// ------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddTransient<AddressDataSeeder>();  
// Enable dynamic JSON serialization BEFORE registering DbContext
NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();

// ------------------------------
// Register Controllers
// ------------------------------
builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<ApartmentRoomValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ApartmentValidator>();
builder.Services.AddFluentValidationAutoValidation();
// ------------------------------
// Configure MoMo Payment
// ------------------------------
builder.Services.Configure<Payments.MoMo.Models.MoMoConfig>(builder.Configuration.GetSection("MoMo"));
builder.Services.AddHttpClient<Payments.MoMo.Services.IMoMoPaymentService, Payments.MoMo.Services.MoMoPaymentService>();

// ------------------------------
// Configure VnPay Payment
// ------------------------------
// builder.Services.AddVnPayPaymentServices(builder.Configuration);

// ------------------------------
// Register custom modules
// ------------------------------
builder.Services.AddAccountModule();
builder.Services.AddAuthModule();
builder.Services.RealEstateModule();
builder.Services.AddTenantModule();
builder.Services.AiModule();
builder.Services.AddressModule();
builder.Services.AddConsumerContactModule();

// Register repository

// ------------------------------
// Configure CORS
// ------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // đổi thành URL frontend của bạn
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ------------------------------
// Add JWT Authentication
// ------------------------------
// Register Firebase services
var loggerFactory = LoggerFactory.Create(logging =>
{
    logging.AddConsole();
});
var logger = loggerFactory.CreateLogger("FirebaseInit");

// Khởi tạo Firebase
FirebaseClientConfig.Init(logger);
// HttpClient cho FirebaseRealtimeService
builder.Services.AddHttpClient<IUserChannelNotificationService, FirebaseRealtimeService>();

builder.Services.AddLogging();

builder.Services.AddAuthentication()
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
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// ------------------------------
// Add Cloudinary
// ------------------------------
builder.Services.AddSingleton<CloudinaryStorage>();
builder.Services.AddScoped<AddressService>();
builder.Services.AddScoped<IAddressImportService, AddressImportService>();
// ------------------------------
// Swagger/OpenAPI
// ------------------------------
builder.Services.AddOpenApi();

// ------------------------------
// Configure file upload limits (30MB)
// ------------------------------
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 30 * 1024 * 1024; // 30MB
});

var app = builder.Build();

// ------------------------------
// Middleware order matters
// ------------------------------
app.UseCors("AllowFrontend"); // CORS first
app.UseAuthentication();
app.UseMiddleware<JwtMiddleware>();
app.UseAuthorization();

// ------------------------------
// Swagger only in dev
// ------------------------------
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

// ------------------------------
// Example route
// ------------------------------
app.MapGet("/weatherforecast", () =>
{
    var summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm",
        "Balmy", "Hot", "Sweltering", "Scorching"
    };

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        )).ToArray();

    return forecast;
}).WithName("GetWeatherForecast");

if (args.Length > 0 && args[0] == "seed-addresses")
{
    var filePath = args.Length > 2 && args[1] == "--file" ? args[2] : "Addresses/data/address_data.csv";

    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<AddressDataSeeder>();
    await seeder.SeedAsync(filePath);

    return; // exit after seeding
}

app.Run();

// ------------------------------
// Record
// ------------------------------
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
