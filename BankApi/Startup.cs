using BankApi.Data;
using BankApi.Middleware;
using BankApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;

namespace BankApi
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(_configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<AuthService>();
            services.AddScoped<ClientService>();
            services.AddScoped<PasswordService>();
            services.AddScoped<TokenService>();
            services.AddScoped<BankCardService>();
            services.AddScoped<TransactionService>();
            services.AddScoped<CacheService>();
            services.AddSingleton<RedisService>();

            var redis = ConnectionMultiplexer.Connect("localhost:6379");
            services.AddSingleton<IConnectionMultiplexer>(redis);

            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
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

            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]
                        ?? throw new ArgumentNullException("Jwt:Key is missing"));
            var issuer = _configuration["Jwt:Issuer"]
                        ?? throw new ArgumentNullException("Jwt:Issuer is missing");
            var audience = _configuration["Jwt:Audience"]
                           ?? throw new ArgumentNullException("Jwt:Audience is missing");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

            services.AddAuthorization();
        }

        public void Configure(WebApplication app)
        {
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
                        Email = "admin@123.com",
                        Password = passwordService.HashPassword("strongpass123"),
                        Role = BankApi.Enums.Role.Admin
                    };
                    db.Clients.Add(admin);
                    db.SaveChanges();
                    Console.WriteLine("SEED: Admin created -> admin@123.com / strongpass123");
                }
            }
        }
    }
}
