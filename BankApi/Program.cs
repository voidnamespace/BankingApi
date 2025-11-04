using BankApi.Data;
using BankApi.Middleware;
using BankApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ClientService>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<BankCardService>();
builder.Services.AddScoped<TransactionService>();

builder.Services.AddSingleton(new RedisService("localhost:6379"));



builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "BankApi", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your token"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]
            ?? throw new ArgumentNullException("Jwt:Key is missing"));
var issuer = builder.Configuration["Jwt:Issuer"]
            ?? throw new ArgumentNullException("Jwt:Issuer is missing");
var audience = builder.Configuration["Jwt:Audience"]
               ?? throw new ArgumentNullException("Jwt:Audience is missing");

Console.WriteLine($"JWT Key length: {key.Length} bytes");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,

            ValidateAudience = true,
            ValidAudience = audience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<AppDbContext>();
    var passwordService = services.GetRequiredService<PasswordService>();

    if (!db.Clients.Any())
    {
        var admin = new BankApi.Entities.Client
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            Email = "admin@mail.ru",
            Password = passwordService.HashPassword("strongpass123"),
            Role = BankApi.Enums.Role.Admin
        };
        db.Clients.Add(admin);
        db.SaveChanges();
        Console.WriteLine("SEED: Admin created -> admin@mail.ru / strongpass123");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseMiddleware<SessionMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.Run();
