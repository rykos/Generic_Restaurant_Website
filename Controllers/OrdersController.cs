using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Restaurant_Website.Data;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Restaurant_Website.Models;

namespace Restaurant_Website.Controllers
{
    [Authorize(Roles = "Administrator,Worker")]
    public class OrdersController : Controller
    {
        private readonly OrderContext orderContext;
        private readonly MvcFoodContext foodContext;

        public OrdersController(OrderContext orderContext, MvcFoodContext foodContext)
        {
            this.orderContext = orderContext;
            this.foodContext = foodContext;
        }

        public async Task<IActionResult> Index()
        {
            List<Order> orders = await this.orderContext.Orders.Where(x => !x.Finished).ToListAsync();//Get all usefull orders
            ViewBag.FoodDefinitions = this.CreateFoodDefinitions(orders);//Unique foods;
            return View(orders);
        }

        public async Task<IActionResult> History()
        {
            List<Order> orders = await this.orderContext.Orders.OrderByDescending(o => o.CreationDate).Take(30).ToListAsync();
            ViewBag.FoodDefinitions = this.CreateFoodDefinitions(orders);//Unique foods;
            return View(orders);
        }

        public async Task<IActionResult> FinishOrder(int? id)
        {
            if (id == null || id < 0)
            {
                return BadRequest();
            }
            var Order = await this.orderContext.Orders.FirstOrDefaultAsync(x => x.Id == id);
            if (Order != null)
            {
                Order.Finished = true;
            }
            await this.orderContext.SaveChangesAsync();
            return Ok();
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
                        Food food = this.foodContext.Food.Find(FoodID);
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

        private Dictionary<int, Food> CreateFoodDefinitions(List<Order> orders)
        {
            Dictionary<int, Food> foodDefinitions = new Dictionary<int, Food>();
            foreach (Order order in orders)
            {
                List<CartItem> foodProducts = this.CartItems(order.CartBuffer);//Get all items in cart buffer
                foreach (CartItem item in foodProducts)
                {
                    if (!foodDefinitions.ContainsKey(item.FoodID))//Check if unique
                    {
                        foodDefinitions.Add(item.FoodID, item.Food);//Add unique food definition
                    }
                }
            }
            return foodDefinitions;
        }
    }
}