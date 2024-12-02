using AutoMapper;
using BusinessObjects.Entity;
using DTOs.FavoriteDishDTOs;
using DTOs.FavoriteMealDTOs;
using DTOs.FoodDTOs;

namespace Services.Mappers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            #region Favorite Dish Mappings
            CreateMap<FavoriteDish, FavoriteDishResponse>()
                .ForMember(dest => dest.DishId, opt => opt.MapFrom(src => src.DishId))
                .ForMember(dest => dest.SmartDietUserId, opt => opt.MapFrom(src => src.SmartDietUserId));

            CreateMap<FavoriteDishDTO, FavoriteDish>();
            #endregion

            #region Favorite Meal Mappings
            CreateMap<FavoriteMeal, FavoriteMealResponse>()
                .ForMember(dest => dest.MealId, opt => opt.MapFrom(src => src.MealId))
                .ForMember(dest => dest.SmartDietUserId, opt => opt.MapFrom(src => src.SmartDietUserId));

            CreateMap<FavoriteMealDTO, FavoriteMeal>();
            #endregion

            #region Food Mappings
            CreateMap<FoodDTO, Food>();

            CreateMap<Food, FoodResponse>()
                .ForMember(dest => dest.NutrientCategories, opt => opt
                    .MapFrom(src => src.NutrientCategories ?? new List<NutrientCategory>()))
                .ForMember(dest => dest.FoodAllergies, opt => opt
                    .MapFrom(src => src.FoodAllergies ?? new List<FoodAllergy>()))
                .ForMember(dest => dest.Image, opt => opt
                    .MapFrom(src => src.Image ?? string.Empty));
            #endregion

            #region Food Allergy Mappings
            CreateMap<FoodAllergy, FoodAllergyResponse>()
                .ForMember(dest => dest.AllergenFoodId, opt => opt
                    .MapFrom(src => src.AllergenFoodId))
                .ForMember(dest => dest.AllergenFoodName, opt => opt
                    .MapFrom(src => src.AllergenFood != null ? src.AllergenFood.Name : string.Empty));
            #endregion

            #region Nutrient Category Mappings
            CreateMap<NutrientCategory, NutrientCategoryResponse>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion

        }
    }
}
