using AutoMapper;
using BusinessObjects.Entity;
using BusinessObjects.FixedData;
using DTOs.AuthDTOs;
using DTOs.DishDTOs;
using DTOs.FavoriteDishDTOs;
using DTOs.FavoriteMealDTOs;
using DTOs.FoodDTOs;
using DTOs.FridgeDTOs;
using DTOs.MealDTOs;
using DTOs.RoleDTOs;
using DTOs.SubcriptionDTOs;
using DTOs.UserAllergyDTOs;
using DTOs.UserFeedbackDTOs;
using DTOs.UserPreferenceDTOs;
using DTOs.UserProfileDTos;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Mappers
{
    public class MappingProfiles : Profile
    {
        private List<RegionType> SplitRegionTypes(RegionType combinedType)
        {
            return Enum.GetValues(typeof(RegionType))
                .Cast<RegionType>()
                .Where(r => r != RegionType.None && combinedType.HasFlag(r))
                .ToList();
        }

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

            CreateMap<UserMealInteractionDTO, UserMealInteraction>();
            CreateMap<UserMealInteraction, UserMealInteractionResponse>();
            #endregion meal

            #region User
            // Auth
            CreateMap<SmartDietUser,RegisterRequest>().ReverseMap();
            // User
            CreateMap<UpdateUserProfileRequest,UserProfile>().ReverseMap();
            // UserProfle
            CreateMap<SmartDietUser, RegisterUserWithRoleRequest>().ReverseMap();
            CreateMap<UserProfile, UserProfileResponse>().ReverseMap();
            CreateMap<UserProfileDTO, UserProfile>()
                .ForMember(dest => dest.ProfilePicture, opt => opt.Ignore()); ;
            // role
            CreateMap<IdentityRole, RoleResponse>().ReverseMap();
            #endregion User

            #region Favorite Dish Mappings
            CreateMap<FavoriteDish, FavoriteDishResponse>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Dish.Name))
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.Dish.Image));

            CreateMap<FavoriteDishDTO, FavoriteDish>();
            #endregion 

            #region Favorite Meal Mappings
            CreateMap<FavoriteMeal, FavoriteMealResponse>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Meal.Name))
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.Meal.Image));

            CreateMap<FavoriteMealDTO, FavoriteMeal>();
            #endregion Favorite Meal Mappings

            #region User Preference Mapping
            CreateMap<UserPreference, UserPreferenceResponse>()
                .ForMember(x => x.PrimaryDietType, y => y.MapFrom(src => src.PrimaryDietType.ToString()))
                .ForMember(x => x.PrimaryRegionTypes, y => y.MapFrom(src => SplitRegionTypes(src.PrimaryRegionType)))
                .ForMember(x => x.DailyMealCount, y => y.MapFrom(src => src.DailyMealCount))
                .ForMember(x => x.DishesPerMealCount, y => y.MapFrom(src => src.DishesPerMealCount))
                .ForMember(x => x.MaxCookingTime, y => y.MapFrom(src => src.MaxCookingTime))
                .ForMember(x => x.MaxRecipeDifficulty, y => y.MapFrom(src => src.MaxRecipeDifficulty.ToString()))
                .ForMember(x => x.NotifyLowInventory, y => y.MapFrom(src => src.NotifyLowInventory))
                .ForMember(x => x.LowInventoryThreshold, y => y.MapFrom(src => src.LowInventoryThreshold));

            CreateMap<UserPreferenceDTO, UserPreference>()
                //.ForMember(x => x.PrimaryDietType, y => y.MapFrom(src => Enum.Parse<DietType>(src.PrimaryDietType)))
                //.ForMember(x => x.PrimaryRegionType, y => y.MapFrom(src => Enum.Parse<RegionType>(src.PrimaryRegionType)))
                .ForMember(x => x.DailyMealCount, y => y.MapFrom(src => src.DailyMealCount))
                .ForMember(x => x.DishesPerMealCount, y => y.MapFrom(src => src.DishesPerMealCount))
                .ForMember(x => x.MaxCookingTime, y => y.MapFrom(src => src.MaxCookingTime))
                //.ForMember(x => x.MaxRecipeDifficulty, y => y.MapFrom(src => Enum.Parse<DifficultyLevel>(src.MaxRecipeDifficulty)))
                .ForMember(x => x.NotifyLowInventory, y => y.MapFrom(src => src.NotifyLowInventory))
                .ForMember(x => x.LowInventoryThreshold, y => y.MapFrom(src => src.LowInventoryThreshold));
            #endregion

            #region User Feedback Mappings
            CreateMap<UserFeedback, UserFeedbackResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.SmartDietUserId, opt => opt.MapFrom(src => src.SmartDietUserId))
                .ForMember(dest => dest.StarRating, opt => opt.MapFrom(src => src.StarRating))
                .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Comment));

            CreateMap<UserFeedbackDTO, UserFeedback>();
            #endregion 

            #region fridge
            CreateMap<FridgeDTO, Fridge>();
            CreateMap<FridgeItemDTO, FridgeItem>();
            CreateMap<Fridge, FridgeRespose>();
            CreateMap<FridgeItem, FridgeItemResponse>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Food.Name))
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.Food.Image));
            #endregion fridge

            #region UserAllergy
            CreateMap<UserAllergy, UserAllergyResponse>()
                .ForMember(dest => dest.FoodName, opt => opt.MapFrom(src => src.Food.Name));
            CreateMap<UserAllergyDTO, UserAllergy>();
            #endregion UserAllergy

            #region UserAllergy
            CreateMap<Subcription, SubcriptionResponse>()
                .ForMember(dest => dest.SubscriptionType, opt => opt.MapFrom(src => src.SubscriptionType.ToString()));
            CreateMap<SubcriptionResponse, SubcriptionRequest>();
            CreateMap<SubcriptionRequest, Subcription>();
            #endregion UserAllergy
        }
    }
}
