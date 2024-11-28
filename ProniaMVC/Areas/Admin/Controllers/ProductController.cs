using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaMVC.Areas.Admin.ViewModels;
using ProniaMVC.DAL;

namespace ProniaMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly AppDbContex _contex;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContex contex, IWebHostEnvironment env)
        {
            _contex = contex;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
           var products = await _contex.Products
                .Include(p=>p.Category)
                .Include(p=>p.ProductImages
                .Where(pi=>pi.IsPrimary==true))
                .Select(p=>new GetProductAdminVM 
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryName=p.Category.Name,
                    Image = p.ProductImages[0].Image
                }
                )
                .ToListAsync();

          
          
            return View();
        }
        public IActionResult Create()
        {
            return View();
        }
    }
}
