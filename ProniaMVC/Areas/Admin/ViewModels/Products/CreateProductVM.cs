﻿using ProniaMVC.Models;

namespace ProniaMVC.Areas.Admin.ViewModels
{
    public class CreateProductVM
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string SKU { get; set; }
      
        public int CategoryId { get; set; }
    }
}
