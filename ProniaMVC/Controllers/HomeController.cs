using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaMVC.DAL;
using ProniaMVC.Models;
using ProniaMVC.ViewModels;

namespace ProniaMVC.Controllers
{
    public class HomeController:Controller
    {
        public readonly AppDbContex _contex;
        public HomeController(AppDbContex contex)
        {
            _contex = contex;
                
        }
        public async Task<IActionResult> Index()
        {

            //_contex.Slides.AddRange(slides);
            //_contex.SaveChanges();
            HomeVM homeVM = new HomeVM
            {
                Slides = await _contex.Slides
                .OrderBy(s => s.Order)
                .Take(2)
                .ToListAsync(),

                NewProducts =  await _contex.Products
                .Take(8)
                .Include(p => p.ProductImages.Where(pi => pi.IsPrimary != null))
                .OrderByDescending(p=>p.CreatedAt)
                .ToListAsync()
            };

            return View(homeVM);
        }
    }
}
