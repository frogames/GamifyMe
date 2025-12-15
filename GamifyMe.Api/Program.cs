using GamifyMe.Api.Data;
using GamifyMe.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;


AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// 1. Services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient",
        policy =>
        {
            policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// HttpContext accessor (multi‑tenant)
builder.Services.AddHttpContextAccessor();

// Email service
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

// Database context
builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Currency service (singleton, holds the name of the primary currency)
builder.Services.AddSingleton<CurrencyService>();

// Business Logic Services
builder.Services.AddScoped<ObjectiveService>();
builder.Services.AddScoped<StoreService>();
builder.Services.AddScoped<StoreService>();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<BadgesService>();
builder.Services.AddScoped<ContentImportService>();

// Hosted Services
builder.Services.AddHostedService<InactiveAccountCleanupService>();

// JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("Jwt:Key").Value!)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// Configure ForwardedHeaders for reverse proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                               Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// --- MIGRATION AUTOMATIQUE ---
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    try
    {
        Console.WriteLine("Applying database migrations...");
        Console.WriteLine("Applying database migrations...");
        dbContext.Database.Migrate();
        Console.WriteLine("Database migrations applied successfully.");

        Console.WriteLine("Seeding Badges...");
        BadgeSeeder.SeedAsync(dbContext).Wait();
        Console.WriteLine("Badges seeded.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error applying migrations: {ex.Message}");
        // On ne throw pas ici pour ne pas crasher l'app si la DB est juste inaccessible temporairement
    }
}
// -----------------------------

// 5. Middleware Pipeline
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// IMPORTANT: UseCors must be before UseStaticFiles
app.UseCors("AllowBlazorClient");

//app.UseHttpsRedirection(); // Disabled in production to avoid port detection errors
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();