using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using System.Text;
using ExportService.Services;

var builder = WebApplication.CreateBuilder(args);

// QuestPDF License
QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddControllers();

builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", builder => {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Register HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Configure JWT Authentication
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

// Configure HttpClients
var servicesConfig = builder.Configuration.GetSection("Services");
builder.Services.AddHttpClient("ResumeService", client =>
{
    client.BaseAddress = new Uri(servicesConfig["ResumeService"] ?? "http://localhost:5255");
});
builder.Services.AddHttpClient("TemplateService", client =>
{
    client.BaseAddress = new Uri(servicesConfig["TemplateService"] ?? "http://localhost:5108");
});


// Register Custom Services
builder.Services.AddScoped<IExportService, ExportService.Services.ExportServiceImplementation>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
