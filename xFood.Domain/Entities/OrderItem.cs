using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace xFood.Domain.Entities
{
	public class OrderItem
	{
		[Key]
		public int Id { get; set; }

		public int OrderId { get; set; }
		public virtual Order Order { get; set; }

		public int ProductId { get; set; }

		[Required, StringLength(150)]
		public string ProductName { get; set; } // Copia do nome (caso o produto mude depois)

		[Column(TypeName = "decimal(18,2)")]
		public decimal UnitPrice { get; set; }

		public int Quantity { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal Total { get; set; }
	}
}