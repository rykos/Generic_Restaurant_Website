using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Microsoft.Net.Http;
using Restaurant_Website.Data;
using Restaurant_Website.Models;
using Microsoft.AspNetCore.Identity;

namespace Restaurant_Website.Controllers
{
    public class LoginController : Controller
    {
        private readonly UserContext _context;
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public LoginController(UserContext context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            this._context = context;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Login([Bind("Id,Username,Password")] User user)
        {
            var dbUser = this.userManager.FindByNameAsync(user.Username);
            if (dbUser != null)
            {
                var signInResult = await this.signInManager.PasswordSignInAsync(user.Username, user.Password, false, false);

                if (signInResult.Succeeded)
                {
                    return RedirectToAction("Index", "MainMenu");
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return NotFound();
            }
        }

        private bool Verify(User user)
        {

            // User dbUser = _context.Users.FirstOrDefault(x => x.Username == user.Username);
            // if (dbUser != null)
            // {
            //     if (dbUser.Password == user.Password)
            //     {
            //         return true;
            //     }
            // }
            return false;
        }

        [HttpPost]
        public async Task<IActionResult> Register([Bind("Email,Username,Password")] User user)
        {
            bool adminRoleExists = await this.roleManager.RoleExistsAsync("Administrator");
            if (!adminRoleExists)
            {
                await this.roleManager.CreateAsync(new IdentityRole("Administrator"));
            }

            IdentityUser newUser = new IdentityUser()
            {
                UserName = user.Username,
                Email = user.Email,
            };

            var result = await this.userManager.CreateAsync(newUser, user.Password);
            if (result.Succeeded)
            {
                if (user.Username == "admin")
                {
                    await this.userManager.AddToRoleAsync(newUser, "Administrator");
                }
                var signInResult = await this.signInManager.PasswordSignInAsync(newUser, user.Password, false, false);
                if (signInResult.Succeeded)
                {
                    return RedirectToAction("Index", "MainMenu");
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> Logout()
        {
            // IdentityUser user = await this.userManager.GetUserAsync(this.User);
            // await this.userManager.AddToRoleAsync(user, "Administrator");
            await this.signInManager.SignOutAsync();
            return RedirectToAction("Index", "MainMenu");
        }

        public IActionResult Register()
        {
            if (this.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "MainMenu");
            }
            return View();
        }

        public IActionResult Login()
        {
            if (this.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "MainMenu");
            }
            return View();
        }
    }
}