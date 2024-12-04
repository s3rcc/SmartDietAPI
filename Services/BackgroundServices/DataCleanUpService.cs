//using BusinessObjects.Base;
//using BusinessObjects.Entity;
//using DataAccessObjects;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Repositories.Interfaces;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;


//namespace Services.BackgroundServices
//{
//    public class DataCleanUpService : BackgroundService
//    {
//        private readonly IServiceProvider _serviceProvider;
//        private readonly IConfiguration _configuration;
//        private readonly ILogger<BackgroundService> _logger;
//        public DataCleanUpService(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<BackgroundService> logger)
//        {
//            _serviceProvider = serviceProvider;
//            _configuration = configuration;
//            _logger = logger;
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            while (!stoppingToken.IsCancellationRequested)
//            {
//                try
//                {
//                    _logger.LogInformation("Data cleanup service is starting");
//                    await CleanupOldDataAsync();
//                }
//                catch (Exception ex)
//                {
//                    // Log exception (replace with your logging framework)
//                    _logger.LogError($"Data cleanup failed: {ex.Message}");
//                }

//                // Run daily
//                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
//            }
//        }

//        private async Task CleanupOldDataAsync()
//        {
//            using (var scope = _serviceProvider.CreateScope())
//            {
//                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

//                // Get cleanup threshold
//                int daysThreshold = _configuration.GetValue<int>("DataCleanup:DaysThreshold");
//                var cutoffDate = DateTime.UtcNow.AddDays(-daysThreshold);

//                // List of entity types to clean up
//                var entityTypes = new[]
//                {
//                    typeof(Food),
//                    typeof(Meal),
//                    typeof(Dish)
//                };

//                foreach (var entityType in entityTypes)
//                {
//                    var repositoryType = typeof(IGenericRepository<BaseEntity>).MakeGenericType(entityType);
//                    dynamic repository = scope.ServiceProvider.GetRequiredService(repositoryType);
//                    // Find item to delete
//                    var itemsToDelete = await repository.FindAsync(x => x.DeletedTime <= cutoffDate);
//                    // Delete Items
//                    if (itemsToDelete.Any())
//                    {
//                        _logger.LogInformation($"{itemsToDelete.Count()} items deleted from database");
//                        repository.DeleteRangeAsync(itemsToDelete);
//                    }
//                }
//                await unitOfWork.SaveChangeAsync();
//            }
//        }
//    }
//}
