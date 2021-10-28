using Microsoft.AspNetCore.Mvc;
using PieShop.Models;
using PieShop.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PieShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPieRepository _pieRepository;

 //I want to show a number of pies on the home page, so again its being passed to me via constructor injection
        public HomeController(IPieRepository pieRepository)
        {
            _pieRepository = pieRepository;
        }
       

        public IActionResult Index()
        {
            //homeViewModel is going to contain a list of PiesOfTheWeek that I get again via the repository that I'm then passing to my view => return View(homeViewModel);
            var homeViewModel = new HomeViewModel
            {
                PiesOfTheWeek = _pieRepository.PiesOfTheWeek 
            };
            return View(homeViewModel);
        }
    }
}
