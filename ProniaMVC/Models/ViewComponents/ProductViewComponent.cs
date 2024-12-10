using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaMVC.DAL;
using ProniaMVC.Utilities.Enums;

namespace ProniaMVC.Models.ViewComponents
{
    public class ProductViewComponent:ViewComponent
    {
        private readonly AppDbContex _context;

        public ProductViewComponent(AppDbContex context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync(SortType sortType)
        {
            List<Product> products = null;
            switch (sortType)
            {
                case SortType.Name:
                  products=  await _context.Products
                .OrderBy(products => products.Name)
                .Take(8)
                .Include(p => p.ProductImages
                .Where(pi => pi.IsPrimary != null))
                .ToListAsync();
                    break;

                case SortType.Price:
                    products = await _context.Products
               .OrderByDescending(products => products.Price)
               .Take(8)
               .Include(p => p.ProductImages
               .Where(pi => pi.IsPrimary != null))
               .ToListAsync();
                    break;
                case SortType.Date:
                    products = await _context.Products
               .OrderBy(products => products.CreatedAt)
               .Take(8)
               .Include(p => p.ProductImages
               .Where(pi => pi.IsPrimary != null))
               .ToListAsync();
                    break;
               
            }


            return View(products);
        }
    }
}
