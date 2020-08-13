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

namespace Restaurant_Website.Controllers
{
    [Authorize(Roles = "Administrator,Worker")]
    public class RestaurantManagerController : Controller
    {
        private readonly WorkTimeContext workTimeContext;
        public RestaurantManagerController(WorkTimeContext workTimeContext) => this.workTimeContext = workTimeContext;

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

        public IActionResult Accounts()
        {
            return View();
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
                    workTime.Start = DateTime.TryParseExact(data["start-time_" + day], "H:mm", null, System.Globalization.DateTimeStyles.None, out tempDate) ? tempDate : default;
                    workTime.End = DateTime.TryParseExact(data["end-time_" + day], "H:mm", null, System.Globalization.DateTimeStyles.None, out tempDate) ? tempDate : default;
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
                return Ok();
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
                    day.Start = workTime.Start;
                    day.End = workTime.End;
                }
                else
                {
                    this.workTimeContext.WorkTimes.AddAsync(workTime);
                }
                this.workTimeContext.SaveChangesAsync();
            }
        }
    }
}