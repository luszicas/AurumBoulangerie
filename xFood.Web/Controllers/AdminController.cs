using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using xFood.Infrastructure.Persistence;
using xFood.Web.Models;

namespace xFood.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly xFoodDbContext _context;

        public AdminController(xFoodDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Segurança Reforçada
            var role = HttpContext.Session.GetString("UserRole");

            // Se não tiver cargo OU se o cargo não for Admin nem Manager
            if (string.IsNullOrEmpty(role) || (role != "Admin" && role != "Manager"))
            {
                // Redireciona para login ou home (bloqueia acesso)
                return RedirectToAction("Login", "Account");
            }

            // 2. Coletando os dados (KPIs)
            var model = new DashboardViewModel();

            model.PendingOrders = await _context.Orders
                .CountAsync(o => o.Status == "Pendente");

            model.LowStockProducts = await _context.Products
                .CountAsync(p => p.Stock < 5);

            model.TotalUsers = await _context.Users
                .CountAsync(u => u.Active);

            model.TotalCategories = await _context.Categories.CountAsync();

            var hoje = DateTime.UtcNow.Date;
            model.TodayRevenue = await _context.Orders
                .Where(o => o.OrderDate >= hoje && o.Status != "Cancelado")
                .SumAsync(o => o.TotalAmount);

            model.RecentOrders = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            return View(model);
        }
    }
}