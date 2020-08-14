using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Restaurant_Website.Data;
using Microsoft.AspNetCore.Http;
using Restaurant_Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;

namespace Restaurant_Website.Controllers
{
    [Authorize(Roles = "Administrator,Worker")]
    public class RestaurantManagerController : Controller
    {
        private readonly WorkTimeContext workTimeContext;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IAccountLogic accountLogic;

        public RestaurantManagerController(WorkTimeContext workTimeContext, UserManager<IdentityUser> userManager, IAccountLogic accountLogic)
        {
            this.workTimeContext = workTimeContext;
            this.userManager = userManager;
            this.accountLogic = accountLogic;
        }

        public IActionResult Index()
        {
            return RedirectToAction("worktimes");
        }

        public IActionResult RestaurantDetails()
        {
            return View();
        }

        public IActionResult WorkTimes()
        {
            return View();
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult Accounts()
        {
            return View(this.userManager.Users.ToList());
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UpdateProfile(IFormCollection data)
        {
            IdentityUser user = await this.userManager.FindByIdAsync(data["id"].ToString());
            if (data["username"].ToString() != null)
            {
                this.accountLogic.ChangeUsernameForID(user.Id, data["username"]);
            }
            if (data["email"].ToString() != null)
            {
                this.accountLogic.ChangeEmailForID(user.Id, data["email"]);
            }

            return RedirectToAction("Accounts");
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> UpdateProfilePassword(IFormCollection data)
        {
            if (data["password"].ToString() != null && data["id"].ToString() != null)
            {
                var pv = new PasswordValidator<IdentityUser>();
                var passwordValidation = await pv.ValidateAsync(this.userManager, null, data["password"].ToString());
                if (passwordValidation.Succeeded)
                {
                    this.accountLogic.ChangePasswordForID(data["id"].ToString(), data["password"].ToString());
                }
                else
                {
                    return BadRequest();
                }
            }
            return RedirectToAction("Accounts");
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AccountDetails(string id)
        {
            return View(await this.userManager.FindByIdAsync(id));
        }

        public IActionResult Logout()
        {
            this.accountLogic.Logout();
            return RedirectToAction("Login", "Login");
        }

        public IActionResult EditWorkTimes(IFormCollection data)
        {
            string[] days = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            List<WorkTime> workTimes = new List<WorkTime>();
            foreach (string day in days)
            {
                WorkTime workTime = new WorkTime();
                workTime.Day = day;
                workTime.IsOpen = (data["is-open_" + day] == "on") ? true : false;
                if (workTime.IsOpen)
                {
                    string startTime = data["start-time_" + day];
                    string endTime = data["end-time_" + day];
                    DateTime tempDate;
                    workTime.Start = DateTime.TryParseExact(startTime, "HH:mm", null, System.Globalization.DateTimeStyles.None, out tempDate) ? tempDate : default;
                    workTime.End = DateTime.TryParseExact(endTime, "HH:mm", null, System.Globalization.DateTimeStyles.None, out tempDate) ? tempDate : default;
                }
                else
                {
                    workTime.Start = default;
                    workTime.End = default;
                }
                workTimes.Add(workTime);
            }
            if (workTimes.Count == 7)
            {
                this.SaveWorkTimes(workTimes);
                return RedirectToAction("Index");
            }
            return BadRequest();
        }

        [HttpGet]
        public string GetWorkTimes()
        {
            List<WorkTime> workTimes = this.workTimeContext.WorkTimes.ToList();
            return JsonConvert.SerializeObject(workTimes); ;
        }

        private void SaveWorkTimes(List<WorkTime> workTimes)
        {
            foreach (WorkTime workTime in workTimes)
            {
                if (this.workTimeContext.WorkTimes.FirstOrDefault(x => x.Day == workTime.Day) != default)//Already exists 
                {
                    var day = this.workTimeContext.WorkTimes.FirstOrDefault(x => x.Day == workTime.Day);
                    this.workTimeContext.WorkTimes.Update(day);
                    day.IsOpen = workTime.IsOpen;
                    if (workTime.Start != default)
                        day.Start = workTime.Start;
                    if (workTime.End != default)
                        day.End = workTime.End;
                }
                else
                {
                    this.workTimeContext.WorkTimes.AddAsync(workTime);
                }
                this.workTimeContext.SaveChanges();
            }
        }
    }
}