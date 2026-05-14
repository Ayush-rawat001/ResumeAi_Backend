using AuthService.Data;
using AuthService.Models;
using AuthService.Repositories;
using AuthService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", builder => {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Configure PostgreSQL (Neon DB)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication Flow:
// 1. Client sends credentials to /login.
// 2. Server validates credentials and returns a signed JWT.
// 3. Client includes JWT in the "Authorization: Bearer <token>" header for protected requests.
// 4. This middleware validates the token's signature, issuer, and expiration.
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Register DI Services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthServiceImplementation>();

// Role-based and Policy-based Authorization:
// - Roles: Used for coarse-grained access control (e.g., [Authorize(Roles = "Admin")]).
// - Policies: Used for fine-grained access control based on specific claims (e.g., SubscriptionPlan).
builder.Services.AddAuthorization(options =>
{
    // Policy-based authorization: Create a "PremiumOnly" policy
    options.AddPolicy("PremiumOnly", policy => 
        policy.RequireClaim("SubscriptionPlan", "PREMIUM"));
});

// Configure Swagger for JWT Support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthService API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

// app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Use Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
        if (!context.Users.Any(u => u.Email == "admin@resumeai.com"))
        {
            context.Users.Add(new User {
                Email = "admin@resumeai.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                FullName = "System Admin",
                Role = "Admin"
            });
            context.SaveChanges();
        }
    } catch (Exception ex) {
        Console.WriteLine($"Seeding error: {ex.Message}");
    }
}

app.Run();
