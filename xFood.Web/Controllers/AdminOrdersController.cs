using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using xFood.Infrastructure.Persistence;

namespace xFood.Web.Controllers
{
	public class AdminOrdersController : Controller
	{
		private readonly xFoodDbContext _context;

		public AdminOrdersController(xFoodDbContext context)
		{
			_context = context;
		}

		// ==========================================
		// 🛡️ MÉTODO DE SEGURANÇA (O GUARDA-COSTAS)
		// ==========================================
		// Esse método verifica se tem alguém logado antes de deixar entrar
		private bool IsUserLogged()
		{
			// Verifica se a sessão "UserName" está vazia
			var userName = HttpContext.Session.GetString("UserName");
			return !string.IsNullOrEmpty(userName);
		}

		public async Task<IActionResult> Index()
		{
			// 🔒 BLOCAGEM: Se não estiver logado, manda pro Login
			if (!IsUserLogged())
			{
				return RedirectToAction("Login", "Account");
			}

			// Se passou pelo segurança, executa o código normal...
			var orders = await _context.Orders
				.Include(o => o.Items)
				.OrderByDescending(o => o.OrderDate)
				.ToListAsync();

			return View(orders);
		}

		public async Task<IActionResult> AdvanceStatus(int id)
		{
			// 🔒 BLOCAGEM: Protege essa ação também
			if (!IsUserLogged())
			{
				return RedirectToAction("Login", "Account");
			}

			var order = await _context.Orders.FindAsync(id);
			if (order != null)
			{
				if (order.Status == "Pendente") order.Status = "Em Preparo";
				else if (order.Status == "Em Preparo") order.Status = "Pronto";
				else if (order.Status == "Pronto") order.Status = "Entregue";

				await _context.SaveChangesAsync();
			}
			return RedirectToAction("Index");
		}
	}
}