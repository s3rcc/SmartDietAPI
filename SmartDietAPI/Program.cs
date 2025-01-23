using BusinessObjects.Base;
using BusinessObjects.Entity;
using DataAccessObjects;
using Google.Apis.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Services;
using Services.Configs;
using Services.Interfaces;
using SmartDietAPI.MiddleWare;
using System.Text;


namespace SmartDietAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var configuration  = builder.Configuration;
            // Add services to the container.
            builder.Services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
                });
            builder.Services.AddMemoryCache();
            // Configure Identity
            builder.Services.AddIdentity<SmartDietUser, IdentityRole>(options =>
            {
                // Configure identity options here if needed
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<SmartDietDbContext>()
            .AddDefaultTokenProviders();
            //--------------------------
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.IncludeErrorDetails = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
                    };

                });
            // Configure Services
            builder.Services.ConfigureService(builder.Configuration);
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHttpContextAccessor();
            //------------------CORS---------
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins", policy =>
                {
                    policy.WithOrigins("http://localhost:3000") // Replace with your frontend URL
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });
            //------------------Swagger---------
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartDiet.API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Example: \"Authorization: Bearer {token}\"",
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
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });
            });
            builder.Services.AddSwaggerGenNewtonsoftSupport();
            //----------Authen google---------------------------------------------
            //builder.Services.AddAuthentication(options =>
            //{
            //    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            //    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //})
            //    .AddCookie()
            //   .AddGoogle(option =>
            //   {
            //       option.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENTID") ?? throw new Exception("GOOGLE_CLIENTID is not set");
            //       option.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENTSECRET") ?? throw new Exception("GOOGLE_CLIENTSECRET is not set");
            //       option.CallbackPath = "/signin-google";
            //       option.SaveTokens = true;
            //   });
            //----------------------------------------------------------------------
            builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
                    options.TokenLifespan = TimeSpan.FromMinutes(30));
            //-------------------------Token Provider-------------------------
            builder.Services.AddDistributedMemoryCache(); // Cấu hình cache cho session
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian timeout cho session
                options.Cookie.HttpOnly = true; // Cookie chỉ có thể truy cập từ server
                options.Cookie.IsEssential = true; // Cookie cần thiết cho ứng dụng
            });
            //---------------------------------------------------------------
            var app = builder.Build();
            //seed
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var initialiser = scope.ServiceProvider.GetRequiredService<SeedAccount>();
                    initialiser.InitialiseAsync().Wait();
                    initialiser.SeedAsync().Wait();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Apply CORS in the middleware pipeline
            app.UseCors("AllowAllOrigins");



            app.UseMiddleware<ValidationMiddleware>();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.UseSession();
            app.Run();
        }
    }
}
