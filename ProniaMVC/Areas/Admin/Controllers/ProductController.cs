using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaMVC.Areas.Admin.ViewModels;
using ProniaMVC.Areas.Admin.ViewModels;
using ProniaMVC.DAL;
using ProniaMVC.Models;
using ProniaMVC.Utilities.Extensions;

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
            List<GetProductAdminVM> productVM = await _contex.Products
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

            return View(productVM);
        }
        public async Task<IActionResult> Create()
        {
            CreateProductVM productVM = new CreateProductVM
            {
                Tags= await _contex.Tags.ToListAsync(),
                Categories = await _contex.Categories.ToListAsync()
            };
            return View(productVM);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM productVM)
        {
            productVM.Categories = await _contex.Categories.ToListAsync();
            productVM.Tags = await _contex.Tags.ToListAsync();
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

            if (!productVM.MainPhoto.ValidateType("image/"))
            {
                ModelState.AddModelError("MainPhoto", "File type is incorrect");
                return View(productVM);
            }
            if (!productVM.MainPhoto.ValidateSize(Utilities.Enums.FileSize.MB,1))
            {
                ModelState.AddModelError("MainPhoto", "File size is incorrect");
                return View(productVM);
            }
            if (!productVM.HoverPhoto.ValidateType("image/"))
            {
                ModelState.AddModelError("HoverPhoto", "File type is incorrect");
                return View(productVM);
            }
            if (!productVM.HoverPhoto.ValidateSize(Utilities.Enums.FileSize.MB, 1))
            {
                ModelState.AddModelError("HoverPhoto", "File size is incorrect");
                return View(productVM);
            }
            


            if (productVM.TagIds is not null)
            {
                bool tagResult = productVM.TagIds.Any(tId => !productVM.Tags.Exists(t => t.Id == tId));
                if (tagResult)
                {
                    ModelState.AddModelError(nameof(CreateProductVM.TagIds), "tags are wrong");
                    return View();
                }
            }

            ProductImage main = new()
            {
                Image = await productVM.MainPhoto.CreateFileAsync(_env.WebRootPath,"assets","images","website-images"),
                IsPrimary = true,
                CreatedAt = DateTime.Now,
                IsDeleted = false

            };
            ProductImage hover = new()
            {
                Image = await productVM.HoverPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images"),
                IsPrimary = false,
                CreatedAt = DateTime.Now,
                IsDeleted = false

            };

            Product product = new()
            {
                Name = productVM.Name,
                SKU = productVM.SKU,
                CategoryId = productVM.CategoryId.Value,
                Description = productVM.Description,
                Price = productVM.Price,
                CreatedAt = DateTime.Now,
                IsDeleted = false,
                ProductTags = productVM.TagIds.Select(tId =>new ProductTag { TagId = tId }).ToList(),
                ProductImages = new List<ProductImage> { main, hover}
            };

            if(productVM.TagIds is not null)
            {
                product.ProductTags = productVM.TagIds.Select(tId=>new ProductTag { TagId=tId}).ToList();
            }

            //foreach (int tId in productVM.TagIds)
            //{
            //    _contex.ProductTags.Add(
            //    new ProductTag
            //    {
            //        TagId = tId,
            //        Product = product
            //    });
            //}
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
            Product product = await _contex.Products.Include(p=>p.ProductTags).FirstOrDefaultAsync(p => p.Id == id);
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
                Categories = await _contex.Categories.ToListAsync(),
                Tags= await _contex.Tags.ToListAsync(),
                TagIds = product.ProductTags.Select(pt=>pt.TagId).ToList()
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
            productVM.Tags = await _contex.Tags.ToListAsync();
            if (!ModelState.IsValid)
            {
                return View(productVM);
            }
            Product existed = await _contex.Products.Include(p=>p.ProductTags).FirstOrDefaultAsync(p => p.Id == id);
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

            if (productVM.TagIds is not null)
            {
                bool tagResult = productVM.TagIds.Any(tId => !productVM.Tags.Exists(t => t.Id == tId));
                if (tagResult)
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.TagIds), "tags are wrong");
                    return View(productVM);

                }
            }

            if (productVM.TagIds is null)
            {
                productVM.TagIds = new();
            }
            else
            {
               productVM.TagIds = productVM.TagIds.Distinct().ToList();
            }
          
                _contex.ProductTags
                .RemoveRange(existed.ProductTags
                .Where(pTag => !productVM.TagIds
                .Exists(tId => tId == pTag.Id))
                .ToList());

                _contex.ProductTags.AddRange(productVM.TagIds
                    .Where(tId => !existed.ProductTags.Exists(pTag => pTag.TagId == tId))
                    .ToList()
                    .Select(tId => new ProductTag { TagId = tId, ProductId = existed.Id })
                    );
            
           

           //foreach(ProductTag pTag in existed.ProductTags)
           // {
           //     if (!productVM.TagIds.Exists(tId => tId == pTag.TagId))
           //     {
           //         deletedItems.Add(pTag);
           //     }
           // }
           // _contex.ProductTags.RemoveRange(deletedItems);

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
