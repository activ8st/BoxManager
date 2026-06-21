using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using BoxManager.Data;
using BoxManager.Models;
using Microsoft.AspNetCore.SignalR;
using BoxManager.Hubs;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.IO;
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

        private static void NormalizeTechnicalSheet(TechnicalSheet sheet)
        {
            if (!sheet.HasPrinting)
            {
                sheet.ColorCount = 0;
                sheet.PrintingType = string.Empty;
                sheet.SpecialFinishes = string.Empty;
                sheet.PrintingNotes = string.Empty;
                sheet.ColorCodes = string.Empty;
                sheet.CustomerLogoPath = string.Empty;
            }
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
            ViewBag.CancelledCount = allOrdersList.Count(o => o.Status == OrderStatus.Cancelled);
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
        public async Task<IActionResult> Create(Order order, TechnicalSheet sheet, IFormFile? CustomerLogo)
        {
            order.OrderDate = DateTime.Now;

            // Il referente non viene chiesto nel form: viene preso automaticamente
            // dal contatto del cliente selezionato.
            var customer = await _context.Customers.FindAsync(order.CustomerId);
            order.Referente = customer?.ContactPerson;

            _context.Add(order);
            await _context.SaveChangesAsync();

            if (CustomerLogo != null && CustomerLogo.Length > 0)
            {
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "logos");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(CustomerLogo.FileName)}";
                var filePath = Path.Combine(uploadsPath, fileName);
                await using var stream = new FileStream(filePath, FileMode.Create);
                await CustomerLogo.CopyToAsync(stream);
                sheet.CustomerLogoPath = $"/uploads/logos/{fileName}";
            }

            sheet.OrderId = order.Id;
            NormalizeTechnicalSheet(sheet);
            _context.Add(sheet);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ReceiveNewOrder", order.Id);
            return RedirectToAction(nameof(Details), new { id = order.Id });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id, string? returnUrl)
        {
            if (id == null) return NotFound();
            var order = await _context.Orders
                .Include(o => o.TechnicalSheet)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            ViewBag.Customers = await _context.Customers.ToListAsync();
            var referer = Request.Headers["Referer"].ToString();
            ViewBag.ReturnUrl = returnUrl ?? referer;
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Order order, IFormFile? CustomerLogo, string? returnUrl)
        {
            if (id != order.Id) return NotFound();

            var existingOrder = await _context.Orders
                .Include(o => o.TechnicalSheet)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (existingOrder == null) return NotFound();

            // Remove validation errors for navigation properties that aren't bound by the form
            ModelState.Remove("Customer");
            ModelState.Remove("TechnicalSheet");
            ModelState.Remove("TechnicalSheet.Order");
            if (ModelState.ContainsKey("TechnicalSheet.Id"))
            {
                ModelState.Remove("TechnicalSheet.Id");
            }

            if (!ModelState.IsValid)
            {
                // Debug: log model state errors
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    System.Diagnostics.Debug.WriteLine($"ModelState Error: {error.ErrorMessage}");
                }
                
                ViewBag.Customers = await _context.Customers.ToListAsync();
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.ModelErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return View(order);
            }

            existingOrder.BoxCode = order.BoxCode;
            existingOrder.Quantity = order.Quantity;
            existingOrder.OrderDate = order.OrderDate;

            if (existingOrder.CustomerId != order.CustomerId)
            {
                var newCustomer = await _context.Customers.FindAsync(order.CustomerId);
                existingOrder.Referente = newCustomer?.ContactPerson;
            }
            existingOrder.CustomerId = order.CustomerId;

            var submittedSheet = order.TechnicalSheet;
            if (submittedSheet == null)
            {
                submittedSheet = existingOrder.TechnicalSheet;
            }

            if (submittedSheet != null)
            {
                NormalizeTechnicalSheet(submittedSheet);
            }

            if (existingOrder.TechnicalSheet == null)
            {
                if (submittedSheet != null)
                {
                    submittedSheet.OrderId = existingOrder.Id;
                    NormalizeTechnicalSheet(submittedSheet);
                    existingOrder.TechnicalSheet = submittedSheet;
                    _context.Add(submittedSheet);
                }
            }
            else if (submittedSheet != null)
            {
                var existingSheet = existingOrder.TechnicalSheet;
                existingSheet.Length = submittedSheet.Length;
                existingSheet.Width = submittedSheet.Width;
                existingSheet.Height = submittedSheet.Height;
                existingSheet.CardboardType = submittedSheet.CardboardType;
                existingSheet.WaveType = submittedSheet.WaveType;
                existingSheet.FefcoCode = submittedSheet.FefcoCode;
                existingSheet.HasPrinting = submittedSheet.HasPrinting;
                existingSheet.UnitPrice = submittedSheet.UnitPrice;
                existingSheet.Discount = submittedSheet.Discount;

                if (submittedSheet.HasPrinting)
                {
                    existingSheet.ColorCount = submittedSheet.ColorCount;
                    existingSheet.PrintingType = submittedSheet.PrintingType;
                    existingSheet.SpecialFinishes = submittedSheet.SpecialFinishes;
                    existingSheet.PrintingNotes = submittedSheet.PrintingNotes;
                    existingSheet.ColorCodes = submittedSheet.ColorCodes;

                    if (!string.IsNullOrEmpty(submittedSheet.CustomerLogoPath))
                    {
                        existingSheet.CustomerLogoPath = submittedSheet.CustomerLogoPath;
                    }
                    else if (string.IsNullOrEmpty(existingSheet.CustomerLogoPath))
                    {
                        existingSheet.CustomerLogoPath = string.Empty;
                    }
                }
                else
                {
                    existingSheet.ColorCount = 0;
                    existingSheet.PrintingType = string.Empty;
                    existingSheet.SpecialFinishes = string.Empty;
                    existingSheet.PrintingNotes = string.Empty;
                    existingSheet.ColorCodes = string.Empty;
                    existingSheet.CustomerLogoPath = string.Empty;
                }
            }

            if (submittedSheet != null)
            {
                var calculatedTotal = submittedSheet.UnitPrice * existingOrder.Quantity * (1 - (submittedSheet.Discount / 100));
                existingOrder.TotalPrice = Math.Round(calculatedTotal, 2);
            }

            if (CustomerLogo != null && CustomerLogo.Length > 0)
            {
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "logos");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(CustomerLogo.FileName)}";
                var filePath = Path.Combine(uploadsPath, fileName);
                await using var stream = new FileStream(filePath, FileMode.Create);
                await CustomerLogo.CopyToAsync(stream);

                if (existingOrder.TechnicalSheet != null)
                {
                    existingOrder.TechnicalSheet.CustomerLogoPath = $"/uploads/logos/{fileName}";
                }
            }

            await _context.SaveChangesAsync();

            // Redirect to Details after save (simple and reliable UX)
            return RedirectToAction(nameof(Details), new { id = existingOrder.Id });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var customerIdClaim = User.FindFirst("CustomerId")?.Value;
                if (customerIdClaim == null || !int.TryParse(customerIdClaim, out int customerId) || order.CustomerId != customerId)
                {
                    return Forbid();
                }
                
                if (status != OrderStatus.Cancelled)
                {
                    return Forbid();
                }
            }

            order.Status = status;
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("ReceiveStatusUpdate", id, status.ToString());
            return Ok();
        }
    }
}