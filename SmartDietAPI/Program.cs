using BusinessObjects.Base;
using BusinessObjects.Entity;
using DataAccessObjects;
using Google.Apis.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Services;
using Services.Configs;
using Services.Interfaces;
using SmartDietAPI.MiddleWare;
using System.Reflection;
using System.Text;


namespace SmartDietAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var environment = builder.Environment.EnvironmentName;
            Console.WriteLine($"Environment: {environment}");
            Console.WriteLine($"Connection string: {builder.Configuration.GetConnectionString("DefaultConnection")}");
            //builder.WebHost.UseUrls("https://0.0.0.0:7095");

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
                //Configure identity options here if needed
                options.SignIn.RequireConfirmedAccount = true;
                options.Password.RequireDigit = true;
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

                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
                c.IncludeXmlComments(xmlPath);

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

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins", builder =>
                {
                    builder
                        //.WithOrigins("exp://192.168.2.8:8082",
                        //"http://localhost:19006",
                        //"http://172.168.3.160:19006",
                        //"http://localhost:8082")
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                        //.AllowCredentials();
                });
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

/*                    var dataSeeder = services.GetRequiredService<SeedData>();
                    dataSeeder.InitialiseAsync().Wait();  // Ensure required dependencies are in place
                    dataSeeder.SeedAsync().Wait();*/

                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }
            // Configure the HTTP request pipeline.
            app.UseHttpsRedirection();

            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartDiet API v1");
                // Serve Swagger UI at root in Production
                if (!app.Environment.IsProduction())
                {
                    options.RoutePrefix = string.Empty;
                }
            });
            app.UseCors("AllowAllOrigins");



            app.UseMiddleware<ValidationMiddleware>();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();
            app.MapControllers();
            app.Run();
        }
    }
}



//Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIzZGI0OGQ1MC02YjE4LTQ0MDItODM4MC03ZWQ2YTJiMWIzNGEiLCJlbWFpbCI6ImhhaGFAaGFoYS5jb20iLCJyb2xlIjoiTWVtYmVyIiwibmJmIjoxNzQwODIxNTYzLCJleHAiOjE3NDE0MjYzNjIsImlhdCI6MTc0MDgyMTU2MywiaXNzIjoiQmFja0VuZCIsImF1ZCI6IkJhY2tFbmQifQ.jSwDOyEmRgNxl5IkyJi7FkdJMFFQm5PqAPwycggJdTk