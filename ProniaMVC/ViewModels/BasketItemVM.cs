﻿namespace ProniaMVC.ViewModels
{
    public class BasketItemVM
    {
        public string Image { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }
        public decimal SubTotal { get; set; }
    }
}
