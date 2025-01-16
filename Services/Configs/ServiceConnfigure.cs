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
//using Services.BackgroundServices;

namespace Services
{
    public static class ServiceConnfigure
    {
        public static IServiceCollection ConfigureService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<SmartDietDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            // Email
            services.AddTransient<IEmailService, EmailSevice>();
            //seed
            services.AddScoped<SeedAccount>();
            //
            services.Configure<MealRecommendationSettings>(configuration.GetSection("MealRecommendationSettings"));

            // Unit of work DI
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            // Other service DI
            services.AddScoped<IFoodService, FoodService>();
            services.AddScoped<IDishService, DishService>();
            services.AddScoped<IMealService, MealService>();

            services.AddScoped<IFavoriteMealService, FavoriteMealService>();
            services.AddScoped<IFavoriteDishService, FavoriteDishService>();

            services.AddScoped<ICloudinaryService, CloudinaryService>();
            services.AddScoped<IFridgeService, FridgeService>();

            services.AddScoped<IUserPreferenceService, UserPreferenceService>();
            services.AddScoped<IUserFeedbackService, UserFeedbackService>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IRecommendationService, RecommendationService>();
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

            return services;
        }
    }
}
