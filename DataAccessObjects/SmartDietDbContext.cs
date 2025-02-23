using BusinessObjects.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;


namespace DataAccessObjects
{
    public class SmartDietDbContext : IdentityDbContext<SmartDietUser, IdentityRole, string>
    {
        public SmartDietDbContext(DbContextOptions<SmartDietDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }
        public DbSet<UserAllergy> UserAllergies { get; set; }
        public DbSet<UserFeedback> UserFeedbacks { get; set; }
        public DbSet<FavoriteDish> FavoriteDishes { get; set; }
        public DbSet<FavoriteMeal> FavoriteMeals { get; set; }
        public DbSet<Food> Foods { get; set; }
        public DbSet<FoodAllergy> FoodAllergies { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<DishIngredient> DishIngredients { get; set; }
        public DbSet<Meal> Meals { get; set; }
        public DbSet<MealDish> MealDishes { get; set; }
        public DbSet<MealRating> MealRatings { get; set; }
        public DbSet<NutrientCategory> NutrientCategories { get; set; }
        public DbSet<Fridge> Fridges { get; set; }
        public DbSet<FridgeItem> FridgeItems { get; set; }
        public DbSet<MealRecommendationHistory> MealRecommendationHistories { get; set; }
        public DbSet<UserMealInteraction> UserMealInteractions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FoodAllergy>()
            .HasOne(fa => fa.Food)
            .WithMany(f => f.FoodAllergies)
            .HasForeignKey(fa => fa.FoodId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FoodAllergy>()
                .HasOne(fa => fa.AllergenFood)
                .WithMany()
                .HasForeignKey(fa => fa.AllergenFoodId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
