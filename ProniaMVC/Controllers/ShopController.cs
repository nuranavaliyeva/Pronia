using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaMVC.DAL;
using ProniaMVC.Models;
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
        public IActionResult Index()
        {
            return View();
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
 