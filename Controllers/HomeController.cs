using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BoxManager.Data;
using BoxManager.ViewModels;
using BoxManager.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace BoxManager.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Customer"))
            {
                var customerIdClaim = User.FindFirst("CustomerId")?.Value;
                if (customerIdClaim != null && int.TryParse(customerIdClaim, out int customerId))
                {
                    var customer = await _context.Customers
                        .Include(c => c.Orders)
                        .FirstOrDefaultAsync(c => c.Id == customerId);
                        
                    if (customer != null)
                    {
                        return View("CustomerDashboard", customer);
                    }
                }
                return RedirectToAction("Index", "Orders");
            }

            var viewModel = new DashboardViewModel
            {
                TotalOrders = await _context.Orders.CountAsync(),
                ActiveCustomers = await _context.Customers.CountAsync(),
                EstimatedRevenue = (decimal)await _context.Orders.SumAsync(o => (double)o.TotalPrice),
                OrdersInProduction = await _context.Orders.CountAsync(o => o.Status == OrderStatus.InProduction),
                RecentOrders = await _context.Orders.Include(o => o.Customer).OrderByDescending(o => o.OrderDate).Take(5).ToListAsync(),
                Customers = await _context.Customers.Take(10).ToListAsync()
            };

            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
