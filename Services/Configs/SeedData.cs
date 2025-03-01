using BusinessObjects.Entity;
using BusinessObjects.FixedData;
using DataAccessObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Configs
{
    public class SeedData
    {
        private readonly ILogger<SeedData> _logger;
        private readonly SmartDietDbContext _context;

        public SeedData(ILogger<SeedData> logger, SmartDietDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task InitialiseAsync()
        {
            try
            {
                if (_context.Database.IsSqlServer())
                {
                    if (!_context.Database.CanConnect())
                    {
                        await _context.Database.EnsureDeletedAsync();
                        await _context.Database.MigrateAsync();
                    }
                    else
                    {
                        await _context.Database.MigrateAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing the database.");
                throw;
            }
        }

        public async Task SeedAsync()
        {
            try
            {
                await TrySeedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        private async Task TrySeedAsync()
        {
            _logger.LogInformation("Starting to seed food and dish data...");

            await SeedFoodsAsync();
            //await SeedDishesAsync();
            await SeedMealsAsync();
            await SeedMealDishesAsync();

            _logger.LogInformation("Food and dish data seeding completed.");
        }

        #region Foods
        private async Task SeedFoodsAsync()
        {
            if (!_context.Foods.Any())
            {
                var foods = new List<Food>
        {
            new Food { Name = "Apple", Description = "Fresh apple", StorageGuidelines = "Keep in fridge", ShelfLifeRoomTemp = 7, ShelfLifeRefrigerated = 30, ShelfLifeFrozen = 90, Category = FoodCategory.Fruits },
            new Food { Name = "Banana", Description = "Ripe banana", StorageGuidelines = "Keep at room temperature", ShelfLifeRoomTemp = 5, ShelfLifeRefrigerated = 10, ShelfLifeFrozen = 60, Category = FoodCategory.Fruits },
            new Food { Name = "Carrot", Description = "Fresh carrot", StorageGuidelines = "Keep in fridge", ShelfLifeRoomTemp = 10, ShelfLifeRefrigerated = 60, ShelfLifeFrozen = 180, Category = FoodCategory.Vegetables },
            new Food { Name = "Chicken Breast", Description = "Fresh chicken breast", StorageGuidelines = "Keep in fridge or freezer", ShelfLifeRoomTemp = 0, ShelfLifeRefrigerated = 2, ShelfLifeFrozen = 180, Category = FoodCategory.Meat },
            new Food { Name = "Salmon", Description = "Fresh salmon", StorageGuidelines = "Keep in fridge or freezer", ShelfLifeRoomTemp = 0, ShelfLifeRefrigerated = 2, ShelfLifeFrozen = 120, Category = FoodCategory.Seafood },
            new Food { Name = "Rice", Description = "Uncooked rice", StorageGuidelines = "Store in a cool, dry place", ShelfLifeRoomTemp = 365, ShelfLifeRefrigerated = null, ShelfLifeFrozen = null, Category = FoodCategory.Grains },
            new Food { Name = "Tomato", Description = "Fresh tomato", StorageGuidelines = "Keep at room temperature", ShelfLifeRoomTemp = 7, ShelfLifeRefrigerated = 14, ShelfLifeFrozen = 90, Category = FoodCategory.Vegetables },
            new Food { Name = "Milk", Description = "Fresh milk", StorageGuidelines = "Keep in fridge", ShelfLifeRoomTemp = 1, ShelfLifeRefrigerated = 7, ShelfLifeFrozen = 30, Category = FoodCategory.Dairy },
            new Food { Name = "Eggs", Description = "Fresh eggs", StorageGuidelines = "Keep in fridge", ShelfLifeRoomTemp = 7, ShelfLifeRefrigerated = 30, ShelfLifeFrozen = 90, Category = FoodCategory.Dairy },
            new Food { Name = "Cheese", Description = "Aged cheese", StorageGuidelines = "Keep in fridge", ShelfLifeRoomTemp = 0, ShelfLifeRefrigerated = 90, ShelfLifeFrozen = 365, Category = FoodCategory.Dairy },
            new Food { Name = "Potato", Description = "Fresh potato", StorageGuidelines = "Keep in a cool, dark place", ShelfLifeRoomTemp = 30, ShelfLifeRefrigerated = null, ShelfLifeFrozen = null, Category = FoodCategory.Vegetables },
            new Food { Name = "Onion", Description = "Fresh onion", StorageGuidelines = "Keep in a cool, dark place", ShelfLifeRoomTemp = 30, ShelfLifeRefrigerated = null, ShelfLifeFrozen = null, Category = FoodCategory.Vegetables },
            new Food { Name = "Garlic", Description = "Fresh garlic", StorageGuidelines = "Keep in a cool, dark place", ShelfLifeRoomTemp = 60, ShelfLifeRefrigerated = null, ShelfLifeFrozen = null, Category = FoodCategory.Vegetables },
            new Food { Name = "Yogurt", Description = "Fresh yogurt", StorageGuidelines = "Keep in fridge", ShelfLifeRoomTemp = 0, ShelfLifeRefrigerated = 14, ShelfLifeFrozen = 60, Category = FoodCategory.Dairy },
            new Food { Name = "Beef", Description = "Fresh beef", StorageGuidelines = "Keep in fridge or freezer", ShelfLifeRoomTemp = 0, ShelfLifeRefrigerated = 3, ShelfLifeFrozen = 180, Category = FoodCategory.Meat },
            new Food { Name = "Pork", Description = "Fresh pork", StorageGuidelines = "Keep in fridge or freezer", ShelfLifeRoomTemp = 0, ShelfLifeRefrigerated = 3, ShelfLifeFrozen = 180, Category = FoodCategory.Meat },
            new Food { Name = "Strawberry", Description = "Fresh strawberry", StorageGuidelines = "Keep in fridge", ShelfLifeRoomTemp = 2, ShelfLifeRefrigerated = 7, ShelfLifeFrozen = 90, Category = FoodCategory.Fruits },
            new Food { Name = "Cucumber", Description = "Fresh cucumber", StorageGuidelines = "Keep in fridge", ShelfLifeRoomTemp = 5, ShelfLifeRefrigerated = 14, ShelfLifeFrozen = 60, Category = FoodCategory.Vegetables },
            new Food { Name = "Lettuce", Description = "Fresh lettuce", StorageGuidelines = "Keep in fridge", ShelfLifeRoomTemp = 0, ShelfLifeRefrigerated = 7, ShelfLifeFrozen = 30, Category = FoodCategory.Vegetables },
            new Food { Name = "Butter", Description = "Dairy butter", StorageGuidelines = "Keep in fridge", ShelfLifeRoomTemp = 0, ShelfLifeRefrigerated = 60, ShelfLifeFrozen = 180, Category = FoodCategory.Dairy }
        };

                await _context.Foods.AddRangeAsync(foods);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Seeded Foods successfully.");
            }
        }
        #endregion

        //#region Dishs
        //private async Task SeedDishesAsync()
        //{
        //    if (!_context.Dishes.Any())
        //    {
        //        var dishes = new List<Dish>
        //{
        //    new Dish { Name = "Spaghetti Carbonara", Description = "Italian pasta dish", PrepTimeMinutes = 10, CookingTimeMinutes = 20, Difficulty = DifficultyLevel.Medium, RegionType = RegionType.Italian },
        //    new Dish { Name = "Grilled Chicken", Description = "Simple grilled chicken", PrepTimeMinutes = 15, CookingTimeMinutes = 30, Difficulty = DifficultyLevel.Easy, RegionType = RegionType.American },
        //    new Dish { Name = "Beef Stroganoff", Description = "Russian beef dish", PrepTimeMinutes = 20, CookingTimeMinutes = 40, Difficulty = DifficultyLevel.Hard, RegionType = RegionType.Greek },
        //    new Dish { Name = "Sushi", Description = "Japanese sushi rolls", PrepTimeMinutes = 30, CookingTimeMinutes = 0, Difficulty = DifficultyLevel.Hard, RegionType = RegionType.Japanese },
        //    new Dish { Name = "Caesar Salad", Description = "Classic Caesar salad", PrepTimeMinutes = 10, CookingTimeMinutes = 0, Difficulty = DifficultyLevel.Easy, RegionType = RegionType.French },
        //    new Dish { Name = "Chicken Curry", Description = "Spicy chicken curry", PrepTimeMinutes = 20, CookingTimeMinutes = 40, Difficulty = DifficultyLevel.Medium, RegionType = RegionType.Indian },
        //    new Dish { Name = "Pad Thai", Description = "Thai stir-fried noodles", PrepTimeMinutes = 15, CookingTimeMinutes = 25, Difficulty = DifficultyLevel.Medium, RegionType = RegionType.Thai },
        //    new Dish { Name = "Bibimbap", Description = "Korean mixed rice", PrepTimeMinutes = 20, CookingTimeMinutes = 30, Difficulty = DifficultyLevel.Medium, RegionType = RegionType.Korean },
        //    new Dish { Name = "Pho", Description = "Vietnamese noodle soup", PrepTimeMinutes = 30, CookingTimeMinutes = 120, Difficulty = DifficultyLevel.Hard, RegionType = RegionType.Vietnamese },
        //    new Dish { Name = "Peking Duck", Description = "Crispy Chinese roasted duck", PrepTimeMinutes = 60, CookingTimeMinutes = 120, Difficulty = DifficultyLevel.Hard, RegionType = RegionType.Chinese }
        //};

        //        await _context.Dishes.AddRangeAsync(dishes);
        //        await _context.SaveChangesAsync();
        //        _logger.LogInformation("Seeded Dishes successfully.");
        //    }
        //}
        //#endregion

        #region Meals
        private async Task SeedMealsAsync()
        {
            if (!_context.Meals.Any())
            {
                var meals = new List<Meal>
        {
            new Meal { Name = "Grilled Chicken Salad", Description = "Healthy grilled chicken with fresh vegetables", DietType = DietType.Keto, Image = "grilled_chicken_salad.jpg", AverageRating = 4.5 },
            new Meal { Name = "Vegetable Stir Fry", Description = "Stir-fried fresh vegetables with soy sauce", DietType = DietType.Vegan, Image = "vegetable_stir_fry.jpg", AverageRating = 4.2 },
            new Meal { Name = "Spaghetti Bolognese", Description = "Classic Italian pasta with meat sauce", DietType = DietType.Mediterranean, Image = "spaghetti_bolognese.jpg", AverageRating = 4.7 },
            new Meal { Name = "Tofu Curry", Description = "Delicious tofu curry with coconut milk", DietType = DietType.Vegetarian, Image = "tofu_curry.jpg", AverageRating = 4.3 },
            new Meal { Name = "Salmon Teriyaki", Description = "Glazed salmon with teriyaki sauce and rice", DietType = DietType.Pescatarian, Image = "salmon_teriyaki.jpg", AverageRating = 4.6 },
            new Meal { Name = "Quinoa Salad", Description = "Refreshing quinoa salad with mixed greens", DietType = DietType.Vegan, Image = "quinoa_salad.jpg", AverageRating = 4.1 },
            new Meal { Name = "Chicken Alfredo", Description = "Creamy Alfredo pasta with grilled chicken", DietType = DietType.Mediterranean, Image = "chicken_alfredo.jpg", AverageRating = 4.5 },
            new Meal { Name = "Lentil Soup", Description = "Nutritious lentil soup with herbs", DietType = DietType.Vegan, Image = "lentil_soup.jpg", AverageRating = 4.0 },
            new Meal { Name = "Beef Steak with Veggies", Description = "Juicy beef steak with roasted vegetables", DietType = DietType.Mediterranean, Image = "beef_steak.jpg", AverageRating = 4.8 },
            new Meal { Name = "Greek Salad", Description = "Fresh Greek salad with feta cheese", DietType = DietType.Vegetarian, Image = "greek_salad.jpg", AverageRating = 4.4 },
            new Meal { Name = "Eggplant Parmesan", Description = "Baked eggplant with tomato sauce and cheese", DietType = DietType.Vegetarian, Image = "eggplant_parmesan.jpg", AverageRating = 4.3 },
            new Meal { Name = "Miso Soup with Tofu", Description = "Traditional Japanese miso soup", DietType = DietType.Vegan, Image = "miso_soup.jpg", AverageRating = 4.2 },
            new Meal { Name = "Chicken Caesar Wrap", Description = "Grilled chicken wrapped with Caesar dressing", DietType = DietType.Mediterranean, Image = "chicken_caesar_wrap.jpg", AverageRating = 4.6 },
            new Meal { Name = "Shrimp Fried Rice", Description = "Fried rice with shrimp and vegetables", DietType = DietType.Pescatarian, Image = "shrimp_fried_rice.jpg", AverageRating = 4.5 },
            new Meal { Name = "Black Bean Tacos", Description = "Tacos filled with spicy black beans", DietType = DietType.Vegan, Image = "black_bean_tacos.jpg", AverageRating = 4.1 },
            new Meal { Name = "Pumpkin Soup", Description = "Creamy pumpkin soup with spices", DietType = DietType.Vegetarian, Image = "pumpkin_soup.jpg", AverageRating = 4.2 },
            new Meal { Name = "Teriyaki Chicken Bowl", Description = "Grilled chicken with teriyaki sauce and rice", DietType = DietType.Mediterranean, Image = "teriyaki_chicken_bowl.jpg", AverageRating = 4.7 },
            new Meal { Name = "Sushi Platter", Description = "Assorted sushi rolls with fresh fish", DietType = DietType.Pescatarian, Image = "sushi_platter.jpg", AverageRating = 4.9 },
            new Meal { Name = "Chickpea Stew", Description = "Rich and hearty chickpea stew", DietType = DietType.Vegan, Image = "chickpea_stew.jpg", AverageRating = 4.3 },
            new Meal { Name = "Oatmeal with Fruits", Description = "Healthy oatmeal with mixed fruits", DietType = DietType.Vegetarian, Image = "oatmeal_fruits.jpg", AverageRating = 4.0 }
        };

                await _context.Meals.AddRangeAsync(meals);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Seeded Meals successfully.");
            }
        }
        #endregion

        #region MealDish
        private async Task SeedMealDishesAsync()
        {
            if (!_context.MealDishes.Any())
            {
                var meals = await _context.Meals.ToListAsync();
                var dishes = await _context.Dishes.ToListAsync();
                var mealDishes = new List<MealDish>();
                var random = new Random();

                // Ensure at least 20 rows by randomly assigning dishes to meals
                foreach (var meal in meals)
                {
                    int numberOfDishes = random.Next(1, 4); // Each meal gets 1 to 3 dishes
                    var selectedDishes = dishes.OrderBy(x => random.Next()).Take(numberOfDishes);

                    foreach (var dish in selectedDishes)
                    {
                        mealDishes.Add(new MealDish
                        {
                            MealId = meal.Id,
                            DishId = dish.Id,
                            ServingSize = random.Next(1, 4) // Serving size between 1 and 3
                        });
                    }
                }

                // Ensure at least 20 records
                while (mealDishes.Count < 20)
                {
                    var meal = meals[random.Next(meals.Count)];
                    var dish = dishes[random.Next(dishes.Count)];

                    mealDishes.Add(new MealDish
                    {
                        MealId = meal.Id,
                        DishId = dish.Id,
                        ServingSize = random.Next(1, 4)
                    });
                }

                await _context.MealDishes.AddRangeAsync(mealDishes);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Seeded MealDishes successfully.");
            }
        }
        #endregion
    }
}