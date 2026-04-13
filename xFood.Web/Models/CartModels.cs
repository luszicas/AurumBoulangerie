namespace xFood.Web.Models
{
	// Representa um único produto na sacola (ex: 2 Croissants)
	public class CartItem
	{
		public int ProductId { get; set; }
		public string ProductName { get; set; }
		public decimal Price { get; set; }
		public int Quantity { get; set; }
		public string ImageUrl { get; set; }

		public decimal Total => Price * Quantity;
	}

	// Representa a sacola inteira
	public class ShoppingCartViewModel
	{
		public List<CartItem> Items { get; set; } = new List<CartItem>();

		public decimal GrandTotal => Items.Sum(i => i.Total);

		public int TotalItems => Items.Sum(i => i.Quantity);
	}
}