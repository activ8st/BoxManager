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

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return RedirectToAction("Index", "Orders");
                }
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty, "Inserire un indirizzo e-mail valido.");
                return View();
            }

            var cleanEmail = email.Trim().ToLower();

            if (cleanEmail == "admin@boxmanager.it")
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "Admin"),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Role, "Admin")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Home");
            }

            // Cerca l'email tra i referenti degli ordini
            var orderWithReferent = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Referente.ToLower() == cleanEmail);

            Customer customer = null;
            if (orderWithReferent != null)
            {
                customer = orderWithReferent.Customer;
            }
            else
            {
                // Cerca l'email tra i contatti dei clienti
                customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email.ToLower() == cleanEmail);
            }

            if (customer != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, customer.ContactPerson),
                    new Claim(ClaimTypes.Email, cleanEmail),
                    new Claim(ClaimTypes.Role, "Customer"),
                    new Claim("CustomerId", customer.Id.ToString())
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Orders");
            }

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
