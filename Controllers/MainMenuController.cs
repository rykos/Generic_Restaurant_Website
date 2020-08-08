using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Restaurant_Website.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Restaurant_Website.Models;
using Microsoft.AspNetCore.Http;

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
            ViewBag.CartBuffer = this.CartItems(HttpContext.Session.GetString("CartBuffer"));
            return View(await _foodContext.Food.FirstOrDefaultAsync(x => x.Id == id));
        }

        public IActionResult AddToCart(int? id)
        {
            if (id == null)
                return NotFound();
            Food food = this._foodContext.Food.Find(id);
            string cartBuffer = HttpContext.Session.GetString("CartBuffer");
            HttpContext.Session.SetString("CartBuffer", cartBuffer + $"{id},1;");
            return RedirectToAction("Product", "MainMenu", new { id = id });
        }

        private List<CartItem> CartItems(string buffer)
        {
            //[foodid,amount;foodid,amount]
            if (buffer == null)
            {
                return null;
            }
            List<CartItem> cartItems = new List<CartItem>();
            string[] elements = buffer.Split(';');
            foreach (string element in elements)
            {
                string[] foodData = element.Split(',');
                if (foodData.Length == 2)
                {
                    int FoodID = int.Parse(foodData[0]);
                    CartItem ci = cartItems.FirstOrDefault(x => x.FoodID == FoodID);
                    if (ci != default)
                    {
                        ci.Amount += int.Parse(foodData[1]);
                    }
                    else
                    {
                        Food food = _foodContext.Food.Find(FoodID);
                        cartItems.Add(new CartItem()
                        {
                            FoodID = int.Parse(foodData[0]),
                            Amount = int.Parse(foodData[1]),
                            Food = food
                        });
                    }
                }
            }
            return cartItems;
        }
    }
}