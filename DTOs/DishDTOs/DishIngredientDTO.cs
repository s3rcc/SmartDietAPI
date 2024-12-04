﻿using BusinessObjects.Entity;
using BusinessObjects.FixedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.DishDTOs
{
    public class DishIngredientDTO
    {
        public string FoodId { get; set; }
        public double Quantity { get; set; }
    }
}
