using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BoxManager.Data;
using BoxManager.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace BoxManager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var customers = from c in _context.Customers select c;
            if (!string.IsNullOrEmpty(searchString))
            {
                customers = customers.Where(s => s.BusinessName.Contains(searchString) || s.VatNumber.Contains(searchString));
            }
            return View(await customers.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var customer = await _context.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BusinessName,VatNumber,Address,Email,Phone,ContactPerson,Notes")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BusinessName,VatNumber,Address,Email,Phone,ContactPerson,Notes")] Customer customer)
        {
            if (id != customer.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }
    }
}
