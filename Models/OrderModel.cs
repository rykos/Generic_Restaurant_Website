using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant_Website.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderID { get; set; }//Order id in paypal database
        public string PayPalStatus { get; set; }//Transaction status
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public bool Finished { get; set; } = false;//Order is finished and should not be displayed to workers
        public decimal Value { get; set; }//Total amount charged
        public string CartBuffer { get; set; }//Buffer used to assemble cart
        public string PayerName { get; set; }
        public string PayerLastName { get; set; }
        public string PayerEmail { get; set; }
        public string PayerPhone { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]//Specify to autogenerate value on creation of this row, default value set to DateTime.Now
        public DateTime CreationDate { get; set; } = DateTime.Now;
    }
}