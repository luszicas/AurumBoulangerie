using xFood.Domain.Entities;

namespace xFood.Web.Models
{
    public class DashboardViewModel
    {
        public int PendingOrders { get; set; }
        public int LowStockProducts { get; set; }
        public int TotalUsers { get; set; }
        public int TotalCategories { get; set; }
        public decimal TodayRevenue { get; set; } // Faturamento do dia
        public List<Order> RecentOrders { get; set; } = new List<Order>();
    }
}