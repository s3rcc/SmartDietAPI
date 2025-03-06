using DataAccessObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Interfaces;
using Repositories;
using Services.Interfaces;
using Services.Mappers;
using System.Reflection;
using Services.Configs;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BusinessObjects.Base;
using BusinessObjects.Entity;
using Net.payOS;


namespace Services
{
    public static class ServiceConnfigure
    {
        public static IServiceCollection ConfigureService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<SmartDietDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"), 
                    b => b.MigrationsAssembly("SmartDietAPI")));
            // Email
            services.AddTransient<IEmailService, EmailSevice>();
            //seed
            services.AddScoped<SeedAccount>();
                //services.AddScoped<SeedData>();
            //
            services.Configure<MealRecommendationSettings>(configuration.GetSection("MealRecommendation"));
            // Unit of work DI
            services.AddScoped<Net.payOS.PayOS>(provider =>
            {
                // Lấy các giá trị từ cấu hình (appsettings.json hoặc environment variables)
                string clientId = configuration["PAYOS_CLIENT_ID"] ?? throw new Exception("Cannot find PAYOS_CLIENT_ID in configuration");
                string apiKey = configuration["PAYOS_API_KEY"] ?? throw new Exception("Cannot find PAYOS_API_KEY in configuration");
                string checksumKey = configuration["PAYOS_CHECKSUM_KEY"] ?? throw new Exception("Cannot find PAYOS_CHECKSUM_KEY in configuration");

                // Trả về instance của PayOS
                return new Net.payOS.PayOS(clientId, apiKey, checksumKey);
            });
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IPaymentService, PaymentService>();
            // Other service DI
            services.AddScoped<IFoodService, FoodService>();
            services.AddScoped<IDishService, DishService>();
            services.AddScoped<IMealService, MealService>();

            services.AddScoped<IFavoriteMealService, FavoriteMealService>();
            services.AddScoped<IFavoriteDishService, FavoriteDishService>();

            services.AddScoped<ICloudinaryService, CloudinaryService>();
            services.AddScoped<IFridgeService, FridgeService>();

            services.AddScoped<IUserPreferenceService, UserPreferenceService>();
            services.AddScoped<IUserAllergyService, UserAllergyService>();
            services.AddScoped<IUserFeedbackService, UserFeedbackService>();
            services.AddScoped<IUserProfileService, UserProfileService>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IRecommendationService, RecommendationService>();
            services.AddScoped<ISubcriptionService, SubcriptionService>();

            // Add new file handling services
            services.AddScoped<IFileHandlerService, FileHandlerService>();
            services.AddScoped<IExcelImportService<Meal>, ExcelImportService<Meal>>();
            services.AddScoped<IExcelImportService<Dish>, ExcelImportService<Dish>>();
            services.AddScoped<IExcelImportService<Food>, ExcelImportService<Food>>();

            // jwt middleware
            services.AddSingleton<TokenValidationParameters>(provider =>
            {
                return new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidAudience = configuration["Jwt:Issuer"],
                    ValidIssuer = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
                    ClockSkew = TimeSpan.FromMinutes(60)
                };
            });
            // Background service
            //services.AddHostedService<DataCleanUpService>();
            //AutoMapper
            services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(Assembly.GetExecutingAssembly());
                cfg.AllowNullDestinationValues = true;
                cfg.AllowNullCollections = true;
            });

            services.AddHttpContextAccessor();

            return services;
        }
    }
}
