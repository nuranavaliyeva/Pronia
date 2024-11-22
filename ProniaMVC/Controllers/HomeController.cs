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
        public IActionResult Index()
        {

            //_contex.Slides.AddRange(slides);
            //_contex.SaveChanges();
            HomeVM homeVM = new HomeVM
            {
                Slides = _contex.Slides.OrderBy(s => s.Order).Take(2).ToList(),
                Products=_contex.Products.Include(p=>p.ProductImages).ToList() 
            };
            return View(homeVM);
        }
    }
}
