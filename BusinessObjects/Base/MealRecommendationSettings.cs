using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Base
{
    public class MealRecommendationSettings
    {
        public PointsConfig Points { get; set; }
        public int MaxMealsPerCategory { get; set; }
        public int DaysToExcludeRecentlyRecommended { get; set; }

        public class PointsConfig
        {
            public int MealRatingPerStar { get; set; }
            public int LikedMeal { get; set; }
            public int DislikedMeal { get; set; }
            public double FavoriteDishPerDish { get; set; }
            public int NewMeal { get; set; }
        }
    }
}
