using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "Admin,Noderator")]
    [AutoValidateAntiforgeryToken]

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
            var productVM = await _contex.Products
                 .Include(p => p.Category)
                 .Include(p => p.ProductImages)
                 .Select(p => new GetProductAdminVM
                 {
                     Id = p.Id,
                     Name = p.Name,
                     Price = p.Price,
                     CategoryName = p.Category.Name,
                     Image = p.ProductImages.FirstOrDefault(p=>p.IsPrimary==true).Image
                 }
                 )
                 .ToListAsync();

            return View(productVM);
        }
        [Authorize]
        public async Task<IActionResult> Create()
        {
            CreateProductVM productVM = new CreateProductVM
            {
                Tags= await _contex.Tags.ToListAsync(),
                Categories = await _contex.Categories.ToListAsync(),
                 Colors= await _contex.Colors.ToListAsync(),
                 Sizes= await _contex.Sizes.ToListAsync(),

            };
            return View(productVM);
        }

        [HttpPost]

        public async Task<IActionResult> Create(CreateProductVM productVM)
        {
            productVM.Categories = await _contex.Categories.ToListAsync();
            productVM.Tags = await _contex.Tags.ToListAsync();
            productVM.Colors= await _contex.Colors.ToListAsync();
            productVM.Sizes  = await _contex.Sizes.ToListAsync();
            
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
            if (productVM.ColorIds is not null)
            {
                bool colorResult = productVM.ColorIds.Any(tId => !productVM.Tags.Exists(t => t.Id == tId));
                if (colorResult)
                {
                    ModelState.AddModelError(nameof(CreateProductVM.ColorIds), "colors are wrong");
                    return View();
                }
            }
            if (productVM.SizeIds is not null)
            {
                bool sizeResult = productVM.SizeIds.Any(tId => !productVM.Sizes.Exists(t => t.Id == tId));
                if (sizeResult)
                {
                    ModelState.AddModelError(nameof(CreateProductVM.SizeIds), "Sizes are wrong");
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
                ProductColors = productVM.ColorIds.Select(cId => new ProductColor { ColorId = cId }).ToList(),
                ProductSizes = productVM.SizeIds.Select(sId => new ProductSize { SizeId = sId }).ToList(),

                ProductImages = new List<ProductImage> { main, hover}
            };

            if(productVM.TagIds is not null)
            {

                product.ProductTags = productVM.TagIds.Select(tId=>new ProductTag { TagId=tId}).ToList();
            }
            if (productVM.ColorIds is not null)
            {

                product.ProductColors = productVM.ColorIds.Select(cId => new ProductColor { ColorId = cId }).ToList();
            }
            if (productVM.SizeIds is not null)
            {
                product.ProductSizes = productVM.SizeIds.Select(sId => new ProductSize { SizeId = sId }).ToList();
            }
           
          if(productVM.AdditionalPhotos is not null)
            {
                string text = string.Empty;
                foreach (IFormFile file in productVM.AdditionalPhotos)
                {
                    if (!file.ValidateType("image/"))
                    {
                        text += $"<p class=\"text-warning\">{file.FileName} type was not correct</p>";
                        continue;
                    }
                    if (!file.ValidateSize(Utilities.Enums.FileSize.MB, 1))
                    {
                        text += $"<p class=\"text-warning\">{file.FileName} size was not correct</p>";
                        continue;
                    }

                    product.ProductImages.Add(new ProductImage
                    {
                        Image = await file.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images"),
                        CreatedAt = DateTime.Now,
                        IsDeleted = false,
                        IsPrimary = null
                    });
                }

                TempData["FileWarning"] = text;

            }
            await _contex.Products.AddAsync(product);
            await _contex.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [Authorize]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int? id)
        {
            if(id == null || id<1)
            {
                return BadRequest();

            }
            Product product = await _contex.Products.Include(p => p.ProductTags).Include(p => p.ProductColors).Include(p=>p.ProductSizes).Include(p=>p.ProductImages).FirstOrDefaultAsync(p => p.Id == id);
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
                Colors = await _contex.Colors.ToListAsync(),
                Sizes = await _contex.Sizes.ToListAsync(),
                TagIds = product.ProductTags.Select(pt=>pt.TagId).ToList(),
                ProductImages = product.ProductImages,
                SizeIds = product.ProductSizes.Select(pt=>pt.SizeId).ToList(),
                ColorIds = product.ProductColors.Select(pt=>pt.ColorId).ToList(),

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
            Product existed = await _contex.Products.Include(p=>p.ProductImages).Include(p => p.ProductTags).Include(p=>p.ProductColors).Include(p=>p.ProductSizes).FirstOrDefaultAsync(p => p.Id == id);
            if (existed == null)
            {
                return NotFound();
            }
            productVM.Categories= await _contex.Categories.ToListAsync();
            productVM.Tags = await _contex.Tags.ToListAsync();
            productVM.Colors = await _contex.Colors.ToListAsync();
            productVM.Sizes = await _contex.Sizes.ToListAsync();

            productVM.ProductImages=existed.ProductImages;
            
            if (!ModelState.IsValid)
            {
                return View(productVM);
            }
           
            if(productVM.MainPhoto != null)
            {
                if (!productVM.MainPhoto.ValidateType("image/"))
                {
                    ModelState.AddModelError("MainPhoto", "File type is incorrect");
                    return View(productVM);
                }
                if (!productVM.MainPhoto.ValidateSize(Utilities.Enums.FileSize.MB, 1))
                {
                    ModelState.AddModelError("MainPhoto", "File size is incorrect");
                    return View(productVM);
                }
            }
            if (productVM.HoverPhoto != null)
            {
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
            }


            if (existed.CategoryId != productVM.CategoryId)
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
            if (productVM.ColorIds is not null)
            {
                bool colorResult = productVM.ColorIds.Any(cId => !productVM.Colors.Exists(c => c.Id == cId));
                if (colorResult)
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.ColorIds), "colors are wrong");
                    return View(productVM);

                }
            }
            if (productVM.SizeIds is not null)
            {
                bool sizeResult = productVM.SizeIds.Any(sId => !productVM.Sizes.Exists(s => s.Id == sId));
                if (sizeResult)
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.SizeIds), "sizes are wrong");
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

            if (productVM.ColorIds is null)
            {
                productVM.ColorIds = new();
            }
            else
            {
                productVM.ColorIds = productVM.ColorIds.Distinct().ToList();
            }

            if (productVM.SizeIds is null)
            {
                productVM.SizeIds = new();
            }
            else
            {
                productVM.SizeIds = productVM.SizeIds.Distinct().ToList();
            }

            _contex.ProductTags
                .RemoveRange(existed.ProductTags
                .Where(pTag => !productVM.TagIds
                .Exists(tId => tId == pTag.Id))
                .ToList());

            _contex.ProductColors
               .RemoveRange(existed.ProductColors
               .Where(pCol => !productVM.ColorIds
               .Exists(cId => cId == pCol.Id))
               .ToList());

            _contex.ProductSizes
               .RemoveRange(existed.ProductSizes
               .Where(pSize => !productVM.SizeIds
               .Exists(sId => sId == pSize.Id))
               .ToList());

            _contex.ProductTags.AddRange(productVM.TagIds
                    .Where(tId => !existed.ProductTags.Exists(pTag => pTag.TagId == tId))
                    .ToList()
                    .Select(tId => new ProductTag { TagId = tId, ProductId = existed.Id }) );

            _contex.ProductColors.AddRange(productVM.ColorIds
                    .Where(cId => !existed.ProductColors.Exists(pCol => pCol.ColorId == cId))
                    .ToList()
                    .Select(cId => new ProductColor { ColorId = cId, ProductId = existed.Id }));


            _contex.ProductSizes.AddRange(productVM.SizeIds
               .Where(sId => !existed.ProductSizes.Exists(pSize => pSize.SizeId == sId))
               .ToList()
               .Select(sId => new ProductSize { SizeId = sId, ProductId = existed.Id }));




            if (productVM.MainPhoto is not null)
            {
                string fileName = await productVM.MainPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images");
                ProductImage main = existed.ProductImages.FirstOrDefault(p=>p.IsPrimary==true);
                main.Image.DeleteFile("");
               existed.ProductImages.Remove(main);
                existed.ProductImages.Add(new ProductImage
                {
                    CreatedAt = DateTime.Now,
                    IsDeleted= false,
                    IsPrimary=true,
                    Image= fileName

                });
            }

            if (productVM.HoverPhoto is not null)
            {
                string fileName = await productVM.HoverPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images");
                ProductImage hover = existed.ProductImages.FirstOrDefault(p => p.IsPrimary == true);
                hover.Image.DeleteFile("");
                existed.ProductImages.Remove(hover);
                existed.ProductImages.Add(new ProductImage
                {
                    CreatedAt = DateTime.Now,
                    IsDeleted = false,
                    IsPrimary = true,
                    Image = fileName

                });
            }

       
            if(productVM.ImageIds is null)
            {
                productVM.ImageIds = new List<int>();
            }
            var deletedImages = existed.ProductImages.Where(pi => !productVM.ImageIds.Exists(imgId => imgId == pi.Id) && pi.IsPrimary == null).ToList();
            deletedImages.ForEach(di => di.Image.DeleteFile(_env.WebRootPath, "assets", "images", "website-images"));
           
                _contex.ProductImages.RemoveRange(deletedImages);
            if (productVM.AdditionalPhotos is not null)
            {
                string text = string.Empty;
                foreach (IFormFile file in productVM.AdditionalPhotos)
                {
                    if (!file.ValidateType("image/"))
                    {
                        text += $"<p class=\"text-warning\">{file.FileName} type was not correct</p>";
                        continue;
                    }
                    if (!file.ValidateSize(Utilities.Enums.FileSize.MB, 1))
                    {
                        text += $"<p class=\"text-warning\">{file.FileName} size was not correct</p>";
                        continue;
                    }

                    existed.ProductImages.Add(new ProductImage
                    {
                        Image = await file.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images"),
                        CreatedAt = DateTime.Now,
                        IsDeleted = false,
                        IsPrimary = null
                    });
                }

                TempData["FileWarning"] = text;

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
