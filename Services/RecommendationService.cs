using BusinessObjects.Base;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly MealRecommendationSettings _settings;
        public RecommendationService(IUnitOfWork unitOfWork, MealRecommendationSettings settings)
        {
            _unitOfWork = unitOfWork;
            _settings = settings;
        }

    }
}
