using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.QuickInfo;
using Microsoft.EntityFrameworkCore;
using ProniaMVC.DAL;
using ProniaMVC.Models;
using ProniaMVC.Utilities.Enums;
using ProniaMVC.Utilities.Exceptions;
using ProniaMVC.ViewModels;

namespace ProniaMVC.Controllers
{
    public class ShopController : Controller
    {
        private readonly AppDbContex _context;

        public ShopController(AppDbContex context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string? search, int? categoryId, int key=1, int page=1)
        {
            IQueryable<Product>query = _context.Products.Include(p=>p.ProductImages.Where(pi=>pi.IsPrimary!=null));
            if(!string.IsNullOrEmpty(search))
            {
                query=query.Where(p=>p.Name.ToLower().Contains(search.ToLower()));
            }
            if(categoryId != null && categoryId > 0)
            {
                query=query.Where(p=>p.CategoryId==categoryId);
            }
            switch (key)
            {
                case (int)SortType.Name:
                    query = query.OrderBy(p => p.Name);
                    break;
                case (int)SortType.Price:
                    query = query.OrderBy(p => p.Price);
                    break;
                case (int)SortType.Date:
                    query = query.OrderBy(p => p.CreatedAt);
                    break;
            }

            int count=query.Count();
            double totalPage = Math.Ceiling((double)count / 3);
            query = query.Skip((page - 1) * 3).Take(3);



            ShopVM shopVM = new ShopVM
            {
                Products = await query.Select(p => new GetProductVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Image = p.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true).Image,
                    SecondaryImage = p.ProductImages.FirstOrDefault(pi => pi.IsPrimary == false).Image,
                    Price = p.Price

                }).ToListAsync(),
                Categories = await _context.Categories.Select(c => new GetCategoryVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    Count = c.Products.Count()
                }).ToListAsync(),
                Search = search,
                CategoryId = categoryId,
                Key = key,
                TotalPage = totalPage,
                CurrentPage = page

            };
            return View(shopVM);
        }
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null || id<1)
            {
                throw new BadRequestException($"{id} id-si yanlisdir");
            }
            Product? product = await _context.Products
                .Include(p=>p.ProductImages.OrderByDescending(pi=>pi.IsPrimary))
                .Include(p=>p.Category)
                .Include(p=>p.ProductTags) 
                .ThenInclude(pt=>pt.Tag)
                //.Include(p=>p.ProductColors)
                //.ThenInclude(p=>p.Color)
                //.Include(p=>p.ProductSizes)
                //.ThenInclude(p=>p.Size)
                .FirstOrDefaultAsync(p=>p.Id == id);

            if (product is null) throw new NotFoundException($"{id} id-li mehsul yoxdur");
            DetailVM detailVM = new DetailVM
            {
                Product = product,
                RelatedProducts = await _context.Products.Where(p => p.CategoryId == product.CategoryId && p.Id != id)
                .Include(p=>p.ProductImages.Where(pi=>pi.IsPrimary!=null))
                .Take(8)
                .ToListAsync()
            };
            return View(detailVM);
        }
    }
}
 