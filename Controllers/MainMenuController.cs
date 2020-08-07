using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Restaurant_Website.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Restaurant_Website.Controllers
{
    public class MainMenuController : Controller
    {
        private readonly MvcFoodContext _foodContext;
        public MainMenuController(MvcFoodContext context)
        {
            this._foodContext = context;
        }

        //Get MainMenu
        public async Task<IActionResult> Index()
        {
            return View(await _foodContext.Food.Where(x => x.Available == true).ToListAsync());
        }

        public async Task<IActionResult> Product(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            return View(await _foodContext.Food.FirstOrDefaultAsync(x => x.Id == id));
        }
    }
}