namespace Restaurant_Website.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int FoodID { get; set; }
        public Food Food { get; set; }
        public int Amount { get; set; }
    }
}