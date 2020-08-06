using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Restaurant_Website.Controllers
{
    public class MainMenuController : Controller
    {
        //Get MainMenu
        public IActionResult Index()
        {
            return View();
        }
    }
}