using AutoMapper;
using BusinessObjects.Entity;
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
            // Food mapping
            CreateMap<FoodDTO, Food>();

            CreateMap<Food, FoodResponse>()
                .ForMember(dest => dest.NutrientCategories, opt => opt
                    .MapFrom(src => src.NutrientCategories ?? new List<NutrientCategory>()))
                .ForMember(dest => dest.FoodAllergies, opt => opt
                    .MapFrom(src => src.FoodAllergies ?? new List<FoodAllergy>()))
                .ForMember(dest => dest.Image, opt => opt
                    .MapFrom(src => src.Image ?? string.Empty));

            // Food Allergy mapping
            CreateMap<FoodAllergy, FoodAllergyResponse>()
                .ForMember(dest => dest.AllergenFoodId, opt => opt
                    .MapFrom(src => src.AllergenFoodId))
                .ForMember(dest => dest.AllergenFoodName, opt => opt
                    .MapFrom(src => src.AllergenFood != null ? src.AllergenFood.Name : string.Empty));

            // Nutrient Category mapping
            CreateMap<NutrientCategory, NutrientCategoryResponse>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
