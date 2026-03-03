//using Microsoft.AspNetCore.Mvc;
//using xFood.Application.Interfaces;
//using xFood.Web.Extensions;
//using xFood.Web.Models;

//namespace xFood.Web.Controllers
//{
//	public class CartController : Controller
//	{
//		private readonly IProductRepository _productRepository;
//		private const string CartSessionKey = "AurumCart";

//		public CartController(IProductRepository productRepository)
//		{
//			_productRepository = productRepository;
//		}

//		public IActionResult Index()
//		{
//			var cart = HttpContext.Session.Get<ShoppingCartViewModel>(CartSessionKey) ?? new ShoppingCartViewModel();
//			return View(cart);
//		}

//		public async Task<IActionResult> Add(int id)
//		{
//			var product = await _productRepository.GetByIdAsync(id);
//			if (product == null) return NotFound();

//			var cart = HttpContext.Session.Get<ShoppingCartViewModel>(CartSessionKey) ?? new ShoppingCartViewModel();
//			var item = cart.Items.FirstOrDefault(i => i.ProductId == id);

//			if (item == null)
//			{
//				cart.Items.Add(new CartItem
//				{
//					ProductId = product.Id,
//					ProductName = product.Name,
//					Price = product.Price,
//					ImageUrl = product.ImageUrl,
//					Quantity = 1
//				});
//			}
//			else
//			{
//				item.Quantity++;
//			}

//			HttpContext.Session.Set(CartSessionKey, cart);
//			return RedirectToAction("Index");
//		}

//		public IActionResult Remove(int id)
//		{
//			var cart = HttpContext.Session.Get<ShoppingCartViewModel>(CartSessionKey);
//			if (cart != null)
//			{
//				var item = cart.Items.FirstOrDefault(i => i.ProductId == id);
//				if (item != null)
//				{
//					cart.Items.Remove(item);
//					HttpContext.Session.Set(CartSessionKey, cart);
//				}
//			}
//			return RedirectToAction("Index");
//		}

//		// --- AQUI ESTÁ A MÁGICA QUE FALTAVA ---
//		[HttpPost]
//		public IActionResult UpdateQuantity(int id, int change)
//		{
//			var cart = HttpContext.Session.Get<ShoppingCartViewModel>(CartSessionKey);

//			// Se não tiver carrinho, retorna erro
//			if (cart == null) return Json(new { success = false });

//			var item = cart.Items.FirstOrDefault(i => i.ProductId == id);

//			if (item != null)
//			{
//				// Atualiza a quantidade
//				item.Quantity += change;

//				// Se zerou, remove o item
//				if (item.Quantity <= 0)
//				{
//					cart.Items.Remove(item);
//				}

//				// Salva na sessão
//				HttpContext.Session.Set(CartSessionKey, cart);

//				// Retorna os dados novos para o JavaScript atualizar a tela
//				return Json(new
//				{
//					success = true,
//					quantity = item.Quantity,
//					itemTotal = item.Total.ToString("C"),
//					grandTotal = cart.GrandTotal.ToString("C"),
//					isRemoved = item.Quantity <= 0
//				});
//			}

//			return Json(new { success = false });
//		}
//		// ---------------------------------------

//		[HttpPost]
//		public IActionResult Checkout(string customerName)
//		{
//			var cart = HttpContext.Session.Get<ShoppingCartViewModel>(CartSessionKey);
//			if (cart == null || !cart.Items.Any())
//			{
//				return RedirectToAction("Index", "Home");
//			}

//			var random = new Random();
//			var orderNumber = random.Next(1000, 9999);

//			ViewBag.OrderNumber = orderNumber;
//			ViewBag.CustomerName = customerName;

//			HttpContext.Session.Remove(CartSessionKey);

//			return View(cart);
//		}
//	}
//}




using Microsoft.AspNetCore.Mvc;
using xFood.Application.Interfaces;
using xFood.Web.Extensions;
using xFood.Web.Models;
using xFood.Infrastructure.Persistence; // Importante
using xFood.Domain.Entities; // Importante

namespace xFood.Web.Controllers
{
	public class CartController : Controller
	{
		private readonly IProductRepository _productRepository;
		private readonly xFoodDbContext _context; // Conexão com o banco
		private const string CartSessionKey = "AurumCart";

		// Injetamos o Contexto aqui no construtor
		public CartController(IProductRepository productRepository, xFoodDbContext context)
		{
			_productRepository = productRepository;
			_context = context;
		}

		public IActionResult Index()
		{
			var cart = HttpContext.Session.Get<ShoppingCartViewModel>(CartSessionKey) ?? new ShoppingCartViewModel();
			return View(cart);
		}

		public async Task<IActionResult> Add(int id)
		{
			var product = await _productRepository.GetByIdAsync(id);
			if (product == null) return NotFound();

			// 1. Verifica se já acabou o estoque antes de qualquer coisa
			if (product.Stock <= 0)
			{
				TempData["Error"] = $"O produto {product.Name} esgotou!";
				return RedirectToAction("Index"); // Ou volta pra Home
			}

			var cart = HttpContext.Session.Get<ShoppingCartViewModel>(CartSessionKey) ?? new ShoppingCartViewModel();
			var item = cart.Items.FirstOrDefault(i => i.ProductId == id);

			if (item == null)
			{
				// Novo item no carrinho
				cart.Items.Add(new CartItem
				{
					ProductId = product.Id,
					ProductName = product.Name,
					Price = product.Price,
					ImageUrl = product.ImageUrl,
					Quantity = 1
					// Como checamos product.Stock <= 0 acima, sabemos que pelo menos 1 tem.
				});
			}
			else
			{
				// Item já existe, vamos aumentar +1
				// 2. A MÁGICA: Verifica se (quantidade atual + 1) ultrapassa o estoque
				if (item.Quantity + 1 > product.Stock)
				{
					TempData["Error"] = $"Desculpe! Temos apenas {product.Stock} unidades de {product.Name} em estoque.";
					return RedirectToAction("Index");
				}

				item.Quantity++;
			}

			HttpContext.Session.Set(CartSessionKey, cart);

			// Opcional: Feedback visual
			TempData["Success"] = "Produto adicionado!";
			return RedirectToAction("Index");
		}


		public IActionResult Remove(int id)
		{
			var cart = HttpContext.Session.Get<ShoppingCartViewModel>(CartSessionKey);
			if (cart != null)
			{
				var item = cart.Items.FirstOrDefault(i => i.ProductId == id);
				if (item != null)
				{
					cart.Items.Remove(item);
					HttpContext.Session.Set(CartSessionKey, cart);
				}
			}
			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<IActionResult> UpdateQuantity(int id, int change)
		{
			var cart = HttpContext.Session.Get<ShoppingCartViewModel>(CartSessionKey);
			if (cart == null) return Json(new { success = false });

			var item = cart.Items.FirstOrDefault(i => i.ProductId == id);
			if (item != null)
			{
				// Busca o estoque atualizado no banco para garantir
				var productInDb = await _productRepository.GetByIdAsync(id);
				if (productInDb == null) return Json(new { success = false });

				// Calcula quanto ficaria a nova quantidade
				var novaQuantidade = item.Quantity + change;

				// Se estiver tentando aumentar (change > 0) e passar do estoque...
				if (change > 0 && novaQuantidade > productInDb.Stock)
				{
					// Retorna erro para o Javascript avisar (precisaria tratar no front)
					// Ou simplesmente TRAVA e não deixa aumentar
					return Json(new
					{
						success = false,
						message = $"Máximo disponível: {productInDb.Stock}"
					});
				}

				item.Quantity += change;
				if (item.Quantity <= 0) cart.Items.Remove(item);

				HttpContext.Session.Set(CartSessionKey, cart);

				return Json(new
				{
					success = true,
					quantity = item.Quantity,
					itemTotal = item.Total.ToString("C"),
					grandTotal = cart.GrandTotal.ToString("C"),
					isRemoved = item.Quantity <= 0
				});
			}
			return Json(new { success = false });
		}


		// --- AQUI É ONDE SALVA O PEDIDO ---
		[HttpPost]
		// No CartController.cs

		[HttpPost]
		public async Task<IActionResult> Checkout(string customerName)
		{
			var cart = HttpContext.Session.Get<ShoppingCartViewModel>(CartSessionKey);
			if (cart == null || !cart.Items.Any()) return RedirectToAction("Index", "Home");

			// 1. INICIA UMA TRANSAÇÃO (Segurança total)
			// Isso garante que a gente só salva o pedido SE conseguir baixar o estoque
			using var transaction = await _context.Database.BeginTransactionAsync();

			try
			{
				var random = new Random();
				var orderNumber = random.Next(1000, 9999);

				// 2. Prepara o Pedido
				var newOrder = new Order
				{
					OrderNumber = orderNumber,
					CustomerName = customerName,
					OrderDate = DateTime.Now,
					TotalAmount = cart.GrandTotal,
					Status = "Pendente",
					Items = cart.Items.Select(i => new OrderItem
					{
						ProductId = i.ProductId,
						ProductName = i.ProductName,
						UnitPrice = i.Price,
						Quantity = i.Quantity,
						Total = i.Total
					}).ToList()
				};

				// 3. Adiciona o Pedido na memória do EF
				_context.Orders.Add(newOrder);

				// =========================================================
				// 📉 AQUI ACONTECE A BAIXA DE ESTOQUE
				// =========================================================
				foreach (var item in cart.Items)
				{
					// Busca o produto original no banco de dados
					var productInDb = await _context.Products.FindAsync(item.ProductId);

					if (productInDb != null)
					{
						// Subtrai a quantidade que o cliente comprou
						productInDb.Stock -= item.Quantity;

						// Segurança: Se o estoque ficar negativo, trava no zero
						if (productInDb.Stock < 0) productInDb.Stock = 0;
					}
				}
				// =========================================================

				// 4. Salva TUDO de uma vez (O Pedido E as alterações de Estoque)
				await _context.SaveChangesAsync();

				// Confirma que deu tudo certo
				await transaction.CommitAsync();

				// 5. Limpa a sessão
				ViewBag.OrderNumber = orderNumber;
				ViewBag.CustomerName = customerName;
				HttpContext.Session.Remove(CartSessionKey);

				return View(cart);
			}
			catch (Exception)
			{
				// Se der qualquer erro, desfaz tudo (não cria pedido nem baixa estoque)
				await transaction.RollbackAsync();
				// Redireciona de volta ou mostra erro (aqui simplifiquei voltando pro index)
				return RedirectToAction("Index");
			}
		}
	}
}