using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaMVC.Areas.Admin.ViewModels;
using ProniaMVC.Areas.Admin.ViewModels;
using ProniaMVC.DAL;
using ProniaMVC.Models;

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
                 .Include(p => p.Category)
                 .Include(p => p.ProductImages
                 .Where(pi => pi.IsPrimary == true))
                 .Select(p => new GetProductAdminVM
                 {
                     Id = p.Id,
                     Name = p.Name,
                     Price = p.Price,
                     CategoryName = p.Category.Name,
                     Image = p.ProductImages[0].Image
                 }
                 )
                 .ToListAsync();

            return View();
        }
        public async Task<IActionResult> Create()
        {
            CreateProductVM productVM = new CreateProductVM
            {
                Categories = await _contex.Categories.ToListAsync()
            };
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM productVM)
        {
            productVM.Categories = await _contex.Categories.ToListAsync();
            if (!ModelState.IsValid)
            {
                return View(productVM);
            }
            bool result = productVM.Categories.Any(c => c.Id == productVM.CategoryId);
            if (!result)
            {
                ModelState.AddModelError(nameof(CreateProductVM.CategoryId), "Category does not exist");
                return View(productVM);
            }

            Product product = new()
            {
                Name = productVM.Name,
                SKU = productVM.SKU,
                CategoryId = productVM.CategoryId.Value,
                Description = productVM.Description,
                Price = productVM.Price,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };
            await _contex.Products.AddAsync(product);
            await _contex.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Update(int? id)
        {
            if(id == null || id<1)
            {
                return BadRequest();

            }
            Product product = await _contex.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            UpdateProductVM productVM = new()
            {
                Name = product.Name,
                SKU = product.SKU,
                CategoryId = product.CategoryId,
                Description = product.Description,
                Price = product.Price,
                Categories = await _contex.Categories.ToListAsync()
            };
            return View(productVM);
           
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id,UpdateProductVM productVM)
        {
            if (id == null || id < 1)
            {
                return BadRequest();

            }
            productVM.Categories= await _contex.Categories.ToListAsync();   
            if(!ModelState.IsValid)
            {
                return View(productVM);
            }
            Product existed = await _contex.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (existed == null)
            {
                return NotFound();
            }

            if(existed.CategoryId != productVM.CategoryId)
            {
                bool result = productVM.Categories.Any(c => c.Id == productVM.CategoryId);
                if (!result)
                {
                    return View(productVM);
                }
            }


            existed.SKU = productVM.SKU;
            existed.Price = productVM.Price;
            existed.CategoryId= productVM.CategoryId.Value;
            existed.Description = productVM.Description;
            existed.Name = productVM.Name;
            

            await _contex.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

           

        }

    }

}
