using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Restaurant_Website.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public string CartItems { get; set; }//[Food_ID,Amount;Food_ID,Amount;Food_ID,Amount;]
        public string Key { get; set; }//Security key
    }
}