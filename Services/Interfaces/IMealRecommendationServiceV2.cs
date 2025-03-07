using AutoMapper;
using BusinessObjects.Base;
using BusinessObjects.Entity;
using BusinessObjects.Enum;
using BusinessObjects.Exceptions;
using DTOs.MealDTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IMealRecommendationServiceV2
    {
        Task<IEnumerable<MealResponse>> GenerateRecommendationsAsync();
        Task<IEnumerable<MealResponse>> RegenerateRecommendationsAsync();
        Task<IEnumerable<MealResponse>> GetRecommendedMealsAsync();
        Task<IEnumerable<MealResponse>> GetRecommendationHistoryAsync();
    }
}
