using BusinessObjects.Base;
using BusinessObjects.Entity;
using BusinessObjects.Enum;

public class UserDishInteraction : BaseEntity
{
    public string SmartDietUserId { get; set; }
    public SmartDietUser SmartDietUser { get; set; }
    public string DishId { get; set; }
    public Dish Dish { get; set; }
    public InteractionType InteractionType { get; set; }
    public DateTime InteractionDate { get; set; }
} 