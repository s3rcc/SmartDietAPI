public class DishRecommendationSettings
{
    public PointsConfig Points { get; set; }
    public int MaxDishesToRecommend { get; set; } = 5;
    public int DaysToExcludeRecentlyRecommended { get; set; }

    public class PointsConfig
    {
        public int DishRatingPerStar { get; set; }
        public int LikedDish { get; set; }
        public int DislikedDish { get; set; }
        public int NewDish { get; set; }
        public double SeasonalBonus { get; set; } // Điểm cộng thêm cho món theo mùa
    }
}
