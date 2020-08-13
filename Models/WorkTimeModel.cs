using System;
using System.ComponentModel.DataAnnotations;

namespace Restaurant_Website.Models
{
    public class WorkTime
    {
        public int Id { get; set; }
        [Required]
        public string Day { get; set; }
        public bool IsOpen { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}