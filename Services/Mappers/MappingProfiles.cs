using AutoMapper;
using BusinessObjects.Entity;
using DTOs.AuthDTOs;
using DTOs.DishDTOs;
using DTOs.FoodDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    .MapFrom(src => src.FoodAllergies));

            // Food Allergy mapping
            CreateMap<FoodAllergy, FoodAllergyResponse>()
                .ForMember(dest => dest.AllergenFoodId, opt => opt
                    .MapFrom(src => src.AllergenFoodId))
                .ForMember(dest => dest.AllergenFoodName, opt => opt
                    .MapFrom(src => src.AllergenFood));

            // Nutrient Category mapping
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
            
            // Auth
            CreateMap<SmartDietUser,RegisterRequest>().ReverseMap();
        }
    }
}
