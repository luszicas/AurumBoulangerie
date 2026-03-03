using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace xFood.Domain.Entities
{
	public class Order
	{
		[Key]
		public int Id { get; set; }

		public int OrderNumber { get; set; }

		[Required, StringLength(150)]
		public string CustomerName { get; set; } = string.Empty; // <--- Inicializado para evitar Warning

		public DateTime OrderDate { get; set; } = DateTime.Now;

		[Column(TypeName = "decimal(18,2)")]
		public decimal TotalAmount { get; set; }

		[StringLength(50)]
		public string Status { get; set; } = "Pendente";

		public List<OrderItem> Items { get; set; } = new();
	}
}