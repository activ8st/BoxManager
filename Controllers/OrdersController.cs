using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BoxManager.Data;
using BoxManager.Models;
using Microsoft.AspNetCore.SignalR;
using BoxManager.Hubs;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.AspNetCore.Authorization;

namespace BoxManager.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<OrderHub> _hubContext;

        public OrdersController(ApplicationDbContext context, IHubContext<OrderHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index(string searchString, string statusFilter)
        {
            var baseOrders = _context.Orders.Include(o => o.Customer).AsQueryable();

            // Se l'utente è un cliente, mostra solo i suoi ordini
            if (User.IsInRole("Customer"))
            {
                var customerIdClaim = User.FindFirst("CustomerId")?.Value;
                if (customerIdClaim != null && int.TryParse(customerIdClaim, out int customerId))
                {
                    baseOrders = baseOrders.Where(o => o.CustomerId == customerId);
                }
            }

            // Conteggi per i tab
            var allOrdersList = await baseOrders.ToListAsync();
            ViewBag.TotalCount = allOrdersList.Count;
            ViewBag.PendingCount = allOrdersList.Count(o => o.Status == OrderStatus.Pending);
            ViewBag.InProductionCount = allOrdersList.Count(o => o.Status == OrderStatus.InProduction);
            ViewBag.CompletedCount = allOrdersList.Count(o => o.Status == OrderStatus.Completed);
            ViewBag.DeliveredCount = allOrdersList.Count(o => o.Status == OrderStatus.Delivered);
            ViewBag.TotalSpent = allOrdersList.Where(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Delivered).Sum(o => o.TotalPrice);

            var filteredOrders = baseOrders;

            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (Enum.TryParse(statusFilter, out OrderStatus statusEnum))
                {
                    filteredOrders = filteredOrders.Where(o => o.Status == statusEnum);
                }
                ViewBag.CurrentStatusFilter = statusFilter;
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                filteredOrders = filteredOrders.Where(o => o.BoxCode.Contains(searchString) || o.Customer.BusinessName.Contains(searchString) || (o.Referente != null && o.Referente.Contains(searchString)));
                ViewBag.CurrentFilter = searchString;
            }
            return View(await filteredOrders.OrderByDescending(o => o.OrderDate).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.TechnicalSheet)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null) return NotFound();

            // Protezione: i clienti possono vedere solo i dettagli dei propri ordini
            if (User.IsInRole("Customer"))
            {
                var customerIdClaim = User.FindFirst("CustomerId")?.Value;
                if (customerIdClaim != null && int.TryParse(customerIdClaim, out int customerId) && order.CustomerId != customerId)
                {
                    return Forbid();
                }
            }

            return View(order);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(int? customerId)
        {
            ViewBag.Customers = await _context.Customers.ToListAsync();
            ViewBag.SelectedCustomerId = customerId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Order order, TechnicalSheet sheet)
        {
            order.OrderDate = DateTime.Now;
            _context.Add(order);
            await _context.SaveChangesAsync();

            sheet.OrderId = order.Id;
            _context.Add(sheet);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ReceiveNewOrder", order.Id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            order.Status = status;
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("ReceiveStatusUpdate", id, status.ToString());
            return Ok();
        }
    }
}
