using AutoMapper;
using BusinessObjects.Entity;
using DTOs.AuthDTOs;
using DTOs.DishDTOs;
using DTOs.FavoriteDishDTOs;
using DTOs.FavoriteMealDTOs;
using DTOs.FoodDTOs;
using DTOs.FridgeDTOs;
using DTOs.MealDTOs;

namespace Services.Mappers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {

            #region food
            // Food mapping
            CreateMap<FoodDTO, Food>()
                .ForMember(dest => dest.FoodAllergies, opt => opt.Ignore())
                .ForMember(dest => dest.Image, opt => opt.Ignore());

            CreateMap<Food, FoodResponse>()
                .ForMember(dest => dest.NutrientCategories, opt => opt
                    .MapFrom(src => src.NutrientCategories))
                .ForMember(dest => dest.FoodAllergies, opt => opt
                    .MapFrom(src => src.FoodAllergies))
                .ForMember(dest => dest.Image, opt => opt
                    .MapFrom(src => src.Image));
            
            CreateMap<FoodAllergy, FoodAllergyResponse>()
                .ForMember(dest => dest.AllergenFoodId, opt => opt
                    .MapFrom(src => src.AllergenFoodId))
                .ForMember(dest => dest.AllergenFoodName, opt => opt
                    .MapFrom(src => src.AllergenFood));

            CreateMap<NutrientCategory, NutrientCategoryResponse>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            #endregion food
            
            #region dish
            // Dish mapping
            CreateMap<DishDTO, Dish>()
                .ForMember(dest => dest.Image, opt => opt.Ignore())
                .ForMember(dest => dest.Video, opt => opt.Ignore())
                .ForMember(dest => dest.DishIngredients, opt => opt.Ignore());

            CreateMap<Dish, DishResponse>()
                .ForMember(dest => dest.DishIngredients, opt => opt
                .MapFrom(src => src.DishIngredients));

            // Dish ingregdient mapping
            CreateMap<DishIngredientDTO, DishIngredient>();

            CreateMap<DishIngredient, DishIngredientResponse>()
                .ForMember(dest => dest.FoodName, opt => opt
                .MapFrom(src => src.Food));
            #endregion dish

            #region meal
            CreateMap<MealDTO, Meal>()
                .ForMember(dest => dest.Image, opt => opt.Ignore());

            CreateMap<Meal, MealResponse>()
                .ForMember(dest => dest.MealDishes, opt => opt
                .MapFrom(src => src.MealDishes))
                .ForMember(dest => dest.Image, opt => opt
                .MapFrom(src => src.Image));

            CreateMap<MealDish, MealDishResponse>()
                .ForMember(dest => dest.Id, opt => opt
                .MapFrom(src => src.DishId))
                .ForMember(dest => dest.Name, opt => opt
                .MapFrom(src => src.Dish.Name))
                .ForMember(dest => dest.Image, opt => opt
                .MapFrom(src => src.Dish.Image));

            #endregion meal

            // Auth
            CreateMap<SmartDietUser,RegisterRequest>().ReverseMap();

            #region Favorite Dish Mappings
            CreateMap<FavoriteDish, FavoriteDishResponse>()
                .ForMember(dest => dest.DishId, opt => opt.MapFrom(src => src.DishId))
                .ForMember(dest => dest.SmartDietUserId, opt => opt.MapFrom(src => src.SmartDietUserId));

            CreateMap<FavoriteDishDTO, FavoriteDish>();
            #endregion Favorite Dish Mappings

            #region Favorite Meal Mappings
            CreateMap<FavoriteMeal, FavoriteMealResponse>()
                .ForMember(dest => dest.MealId, opt => opt.MapFrom(src => src.MealId))
                .ForMember(dest => dest.SmartDietUserId, opt => opt.MapFrom(src => src.SmartDietUserId));

            CreateMap<FavoriteMealDTO, FavoriteMeal>();
            #endregion Favorite Meal Mappings

            #region fridge
            CreateMap<FridgeDTO, Fridge>();
            CreateMap<FridgeItemDTO, FridgeItem>();
            CreateMap<Fridge, FridgeRespose>();
            CreateMap<FridgeItem, FridgeItemResponse>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Food.Name));
            #endregion fridge

        }
    }
}
