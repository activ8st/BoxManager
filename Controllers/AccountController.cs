using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BoxManager.Data;
using BoxManager.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BoxManager.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        private const string AdminEmail    = "admin@boxmanager.it";
        private const string AdminPassword = "Esame2026admin";

        private const string ClientEmail    = "c.bianchi@outlook.it";
        private const string ClientPassword = "Esame2026client";

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User?.Identity?.IsAuthenticated ?? false)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "Inserire email e password.");
                return View();
            }

            var cleanEmail    = email.Trim().ToLower();
            var cleanPassword = password.Trim();

            // ── Admin ────────────────────────────────────────────────────
            if (cleanEmail == AdminEmail.ToLower())
            {
                if (cleanPassword != AdminPassword)
                {
                    ModelState.AddModelError(string.Empty, "Password non corretta.");
                    return View();
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,  "Admin"),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Role,  "Admin")
                };

                var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Home");
            }

            // ── Cliente ──────────────────────────────────────────────────
            if (cleanEmail == ClientEmail.ToLower())
            {
                if (cleanPassword != ClientPassword)
                {
                    ModelState.AddModelError(string.Empty, "Password non corretta.");
                    return View();
                }

                // Cerca il cliente nel DB tramite email
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email.ToLower() == cleanEmail);

                // Se non trovato per email diretta, cerca tramite referente negli ordini
                if (customer == null)
                {
                    var order = await _context.Orders
                        .Include(o => o.Customer)
                        .FirstOrDefaultAsync(o => o.Referente != null && o.Referente.ToLower() == cleanEmail);

                    customer = order?.Customer;
                }

                if (customer == null)
                {
                    ModelState.AddModelError(string.Empty, "Nessun cliente associato a questa email.");
                    return View();
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,  customer.ContactPerson),
                    new Claim(ClaimTypes.Email, cleanEmail),
                    new Claim(ClaimTypes.Role,  "Customer"),
                    new Claim("CustomerId",     customer.Id.ToString())
                };

                var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Home");
            }

            // ── Email non riconosciuta ────────────────────────────────────
            ModelState.AddModelError(string.Empty, "Indirizzo e-mail non registrato nel sistema.");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}