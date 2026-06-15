using BoxManager.Models;
using System.Collections.Generic; 

namespace BoxManager.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalOrders { get; set; }
        public int ActiveCustomers { get; set; }
        public decimal EstimatedRevenue { get; set; }
        public int OrdersInProduction { get; set; }
        public List<Order> RecentOrders { get; set; } = new List<Order>();
        public List<Customer> Customers { get; set; } = new List<Customer>();
    }
}
