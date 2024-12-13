﻿using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProniaMVC.DAL;
using ProniaMVC.Models;
using ProniaMVC.Services.Interfaces;
using ProniaMVC.ViewModels;
using System.Security.Claims;

namespace ProniaMVC.Services.Implementations
{
    public class LayoutServices:ILayoutService
    {
        private readonly AppDbContex _context;
        private readonly IHttpContextAccessor _http;
        private readonly ClaimsPrincipal _user;

        public LayoutServices(AppDbContex context, IHttpContextAccessor http)
        {
            _context = context;
            _http = http;
            _user = _http.HttpContext.User;
        }

        public async Task<List<BasketItemVM>> GetBasketAsync()
        {
            List<BasketItemVM> basketVM = new();
            if (_user.Identity.IsAuthenticated)
            {
                basketVM = await _context.BasketItems.Where(bi => bi.AppUserId == _user.FindFirstValue(ClaimTypes.NameIdentifier)).Select(bi => new BasketItemVM
                {
                    Count = bi.Count,
                    Price = bi.Product.Price,
                    Image = bi.Product.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true).Image,
                    Name = bi.Product.Name,
                    SubTotal = bi.Product.Price * bi.Count,
                    Id = bi.ProductId,

                })
                .ToListAsync();
            }
            else
            {
                List<BasketCookieItemVM> cookiesVM;
                string cookie = _http.HttpContext.Request.Cookies["basket"];

                if (cookie == null)
                {
                    return basketVM;
                }

                cookiesVM = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(cookie);

                foreach (BasketCookieItemVM item in cookiesVM)
                {
                    Product product = await _context.Products.Include(p => p.ProductImages.Where(p => p.IsPrimary == true)).FirstOrDefaultAsync(p => p.Id == item.Id);
                    if (product != null)
                    {
                        basketVM.Add(new BasketItemVM
                        {
                            Id = item.Id,
                            Name = product.Name,
                            Image = product.ProductImages[0].Image,
                            Price = product.Price,
                            Count = item.Count,
                            SubTotal = item.Count * product.Price
                        });
                    }

                }

            }

           
            return basketVM;
        }

        public async Task<Dictionary<string, string>> GetSettingsAsync()
        {
            Dictionary<string, string> settings = await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);
              return settings;
        }
       


    }
}
