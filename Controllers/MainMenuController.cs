using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Restaurant_Website.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Restaurant_Website.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace Restaurant_Website.Controllers
{
    public class MainMenuController : Controller
    {
        private readonly MvcFoodContext _foodContext;
        private readonly OrderContext orderContext;
        private readonly WorkTimeContext workTimeContext;
        private readonly IAccountLogic accountLogic;
        private readonly string _paypalAuthKey = "QVdvcm1EalpoVUlMTGR3NG9oMEJQeWk2empXaE4tLVppbE95SnlEUkFpQVFBdF9EOWtFbHo3S1RzSnNrZTJaT09fTmF2MmNsNDVfR3o4czU6RUE4YTV5SlVpYW9IV2FuVmVnYXJSSjNza1Q2TVRrUWVNWW5ldFZBbnJqLUhoOUplQndFcUFETkRJa3dUYlNVRTB4ekpNYmdWbjlRbUJwclo=";

        public MainMenuController(MvcFoodContext context, OrderContext orderContext, WorkTimeContext workTimeContext, IAccountLogic accountLogic)
        {
            this._foodContext = context;
            this.orderContext = orderContext;
            this.workTimeContext = workTimeContext;
            this.accountLogic = accountLogic;
        }

        //Get MainMenu
        public async Task<IActionResult> Index()
        {
            
            if (!IsWorkingTime())
                return await WeAreNotWorking();
            ViewBag.FoodTypes = this.GetFoodCategory();
            ViewBag.CartBuffer = this.CartItems(HttpContext.Session.GetString("CartBuffer"));
            return View(await _foodContext.Food.Where(x => x.Available == true).ToListAsync());
        }

        public async Task<IActionResult> WeAreNotWorking()
        {
            return View("/Views/MainMenu/NotWorking.cshtml", await this.workTimeContext.WorkTimes.AsNoTracking().ToListAsync());
        }

        public async Task<IActionResult> Product(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            if (!IsWorkingTime())
            {
                return await WeAreNotWorking();
            }
            ViewBag.CartBuffer = this.CartItems(HttpContext.Session.GetString("CartBuffer"));
            return View(await _foodContext.Food.FirstOrDefaultAsync(x => x.Id == id));
        }

        public IActionResult AddToCart(int? id, int? amount = 1)
        {
            if (id == null)
                return NotFound();
            Food food = this._foodContext.Food.Find(id);
            string cartBuffer = HttpContext.Session.GetString("CartBuffer");
            HttpContext.Session.SetString("CartBuffer", cartBuffer + $"{id},{amount};");
            return RedirectToAction("Product", "MainMenu", new { id = id });
        }

        public IActionResult RemoveFromCart(int? id, int? amount = 1)
        {
            if (id == null)
                return NotFound();
            var cartItems = this.CartItems(HttpContext.Session.GetString("CartBuffer"));
            var cartItem = cartItems.FirstOrDefault(x => x.FoodID == id);
            if (cartItem != null)
            {
                cartItem.Amount -= (int)amount;
            }
            else
            {
                return BadRequest();
            }
            string newBuffer = this.CartItemsListToString(cartItems);
            HttpContext.Session.SetString("CartBuffer", newBuffer);
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
        private string CartItemsListToString(List<CartItem> cartItems)
        {
            StringBuilder buffer = new StringBuilder();
            foreach (CartItem item in cartItems)
            {
                if (item != null)
                {
                    buffer.Append($"{item.FoodID},{item.Amount};");
                }
            }
            return buffer.ToString();
        }

        [HttpGet]
        public IActionResult GetCartItems()
        {
            ViewBag.CartBuffer = this.CartItems(HttpContext.Session.GetString("CartBuffer"));
            return PartialView("/Views/Cart/CartPartial.cshtml");
        }

        private string[] GetFoodCategory()
        {
            return _foodContext.Food.Where(p => p.Available).Select(p => p.Type).Distinct().ToArray();
        }

        public decimal GetCartPrice()
        {
            decimal totalprice = 0;
            CartItems(HttpContext.Session.GetString("CartBuffer"))?.ForEach(x =>
            {
                totalprice += x.Food.Price * x.Amount;
            });
            return totalprice;
        }

        public IActionResult Finalyze()
        {
            return View(this.CartItems(HttpContext.Session.GetString("CartBuffer")));
        }

        public async Task<IActionResult> Category(string category)
        {
            ViewBag.FoodTypes = this.GetFoodCategory();
            return View(await _foodContext.Food.Where(p => p.Available && p.Type == category).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> CapturePaypalTransaction(string id)
        {
            //https://api.sandbox.paypal.com/v2/checkout/orders/order_id
            if (id == null || id == "")
            {
                return BadRequest();
            }
            string url = $"https://api.sandbox.paypal.com/v2/checkout/orders/{id}";
            HttpClient client = new HttpClient();
            //Get
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, url);
            //Add authorization
            msg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", _paypalAuthKey);
            //Fetch response
            HttpResponseMessage response = await client.SendAsync(msg);
            //Save response body
            string responseBody = response.Content.ReadAsStringAsync().Result;
            //Process response
            if (this.ProcessCapturedPaypalTransaction(responseBody))
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        ///<summary>Returns success state</summary>
        private bool ProcessCapturedPaypalTransaction(string responseBody)
        {
            Dictionary<string, dynamic> json = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(responseBody);
            if (json["status"] == "COMPLETED")
            {
                Order order = new Order();
                order.OrderID = json["id"];
                order.PayPalStatus = "COMPLETED";
                order.Value = json["purchase_units"][0]["amount"]["value"];
                order.PayerName = json["payer"]["name"]["given_name"];
                order.PayerLastName = json["payer"]["name"]["surname"];
                order.PayerEmail = json["payer"]["email_address"];
                order.PayerPhone = json["payer"]?["phone"]?["phone_number"]?["national_number"];
                order.City = json["purchase_units"][0]["shipping"]["address"]["admin_area_2"];
                order.Address = json["purchase_units"][0]["shipping"]?["address"]?["address_line_1"] + "&" + json["purchase_units"][0]?["shipping"]?["address"]?["address_line_2"];
                order.CartBuffer = HttpContext.Session.GetString("CartBuffer") ?? "FAILED TO FETCH CART BUFFER";
                this.orderContext.Add(order);
                this.orderContext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        private bool IsWorkingTime()
        {
            TimeSpan now = DateTime.Now.TimeOfDay;
            string dayName = DateTime.Now.DayOfWeek.ToString();
            WorkTime workTime = this.workTimeContext.WorkTimes.AsNoTracking().Where(x => x.Day == dayName).FirstOrDefault();

            TimeSpan start = workTime.Start.TimeOfDay;
            TimeSpan end = workTime.End.TimeOfDay;

            if (now >= start && now <= end)//Work hours
            {
                return true;
            }
            else if (end < start)//work extends into next day
            {
                if (now <= end)
                {
                    return true;
                }
            }
            //Closed
            return false;
        }
    }
}