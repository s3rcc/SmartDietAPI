using DataAccessObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Interfaces;
using Repositories;
using Services.Interfaces;
using Services.Mappers;
using System.Reflection;
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
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();

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
