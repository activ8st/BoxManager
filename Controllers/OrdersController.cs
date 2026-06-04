using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BoxManager.Data;
using BoxManager.Models;
using Microsoft.AspNetCore.SignalR;
using BoxManager.Hubs;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace BoxManager.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<OrderHub> _hubContext;

        public OrdersController(ApplicationDbContext context, IHubContext<OrderHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var orders = _context.Orders.Include(o => o.Customer).AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o => o.BoxCode.Contains(searchString) || o.Customer.BusinessName.Contains(searchString));
            }
            return View(await orders.OrderByDescending(o => o.OrderDate).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.TechnicalSheet)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null) return NotFound();
            return View(order);
        }

        public async Task<IActionResult> Create(int? customerId)
        {
            ViewBag.Customers = await _context.Customers.ToListAsync();
            ViewBag.SelectedCustomerId = customerId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order, TechnicalSheet sheet)
        {
            // Simple validation for MVP
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
