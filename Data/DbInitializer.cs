using BoxManager.Models;
using System;
using System.Linq;

namespace BoxManager.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            if (context.Customers.Any())
            {
                return;   // DB has been seeded
            }

            var customers = new Customer[]
            {
                new Customer { BusinessName="Pasta Fresca Lombardi s.r.l.", VatNumber="01234567890", Address="Indirizzo 1", Email="ordini@lombardi-pasta.it", Phone="051 123456", ContactPerson="Marco Lombardi", Notes="Cliente storico" },
                new Customer { BusinessName="Vini Pregiati Barolo S.p.A.", VatNumber="09876543210", Address="Indirizzo 2", Email="acquisti@vinibarolo.it", Phone="0173 654321", ContactPerson="Eleonora Bianchi", Notes="Pagamento 60gg" },
                new Customer { BusinessName="Farnaceutica Ned Italia S.r.l.", VatNumber="04567891230", Address="Indirizzo 3", Email="logistica@meditalia.it", Phone="02 987654", ContactPerson="Sofia Martini", Notes="" },
                new Customer { BusinessName="Salumificio Rossi&Figli", VatNumber="03210987654", Address="Indirizzo 1", Email="ordini@salumificiorossi.it", Phone="0521 456789", ContactPerson="Giovanni Rossi", Notes="" },
                new Customer { BusinessName="Elettronica Furlan S.r.l.", VatNumber="07654312890", Address="Indirizzo 5", Email="magazzino@furlan-el.it", Phone="049 876543", ContactPerson="Paolo Furlan", Notes="" },
                new Customer { BusinessName="Cosmetici Bella Donna", VatNumber="05678901234", Address="Indirizzo 5", Email="packaging@belladonna.it", Phone="055 321654", ContactPerson="Francesca Esposito", Notes="" },
                new Customer { BusinessName="Ceramiche Artigianali Veneziane", VatNumber="06543210987", Address="Indirizzo 4", Email="spedizioni@ceramichevenezia.com", Phone="041 555666", ContactPerson="Luca Morosini", Notes="" },
                new Customer { BusinessName="Abbigliamento Moda Nord S.r.l.", VatNumber="02109876543", Address="Indirizzo 1", Email="acquisti@modanord.it", Phone="011 444555", ContactPerson="Valerio Ferrari", Notes="" }
            };

            foreach (Customer c in customers)
            {
                context.Customers.Add(c);
            }
            context.SaveChanges();

            // Seed 15 orders to match mockup counts:
            // 7 Pending (In attesa), 3 InProduction (In corso), 3 Completed (Pronto), 1 Cancelled (Annullato), 1 Delivered (Consegnato)
            var orders = new Order[]
            {
                // Older background orders for counts:
                new Order { CustomerId=1, OrderDate=DateTime.Now.AddDays(-20), BoxCode="Fefco 1", Quantity=1000, TotalPrice=2200.00m, Status=OrderStatus.Delivered, Referente="m.lombardi@outlook.com" },
                new Order { CustomerId=3, OrderDate=DateTime.Now.AddDays(-15), BoxCode="Fefco 2", Quantity=500, TotalPrice=1100.00m, Status=OrderStatus.Pending, Referente="logistica@meditalia.it" },
                new Order { CustomerId=4, OrderDate=DateTime.Now.AddDays(-12), BoxCode="Fefco 1", Quantity=800, TotalPrice=1600.00m, Status=OrderStatus.Pending, Referente="g.esposito@outlook.com" },
                new Order { CustomerId=7, OrderDate=DateTime.Now.AddDays(-10), BoxCode="Mahsul 4", Quantity=600, TotalPrice=1200.00m, Status=OrderStatus.Pending, Referente="l.morosini@outlook.com" },
                new Order { CustomerId=2, OrderDate=DateTime.Now.AddDays(-8), BoxCode="Fefco 5", Quantity=1200, TotalPrice=3000.00m, Status=OrderStatus.Pending, Referente="e.esposito@outlook.com" },

                // Mockup orders (ORD-2026-006 to ORD-2026-015):
                new Order { CustomerId=5, OrderDate=DateTime.Now.AddDays(-7), BoxCode="Fefco 2", Quantity=1800, TotalPrice=3600.00m, Status=OrderStatus.Completed, Referente="m.lombardi@outlook.com" },
                new Order { CustomerId=6, OrderDate=DateTime.Now.AddDays(-6), BoxCode="Fefco 2", Quantity=500, TotalPrice=1100.00m, Status=OrderStatus.Completed, Referente="e.bianchi@outlook.com" },
                new Order { CustomerId=1, OrderDate=DateTime.Now.AddDays(-5), BoxCode="Fefco 1", Quantity=4000, TotalPrice=9800.00m, Status=OrderStatus.Completed, Referente="m.lombardi@outlook.com" },
                new Order { CustomerId=7, OrderDate=DateTime.Now.AddDays(-4), BoxCode="Mahsul 4", Quantity=300, TotalPrice=950.00m, Status=OrderStatus.Cancelled, Referente="l.morosini@outlook.com" },
                new Order { CustomerId=8, OrderDate=DateTime.Now.AddDays(-3), BoxCode="Fefco 5", Quantity=1000, TotalPrice=2500.00m, Status=OrderStatus.Pending, Referente="v.ferretti@outlook.com" },
                new Order { CustomerId=2, OrderDate=DateTime.Now.AddDays(-2), BoxCode="Fefco 5", Quantity=5000, TotalPrice=12500.00m, Status=OrderStatus.InProduction, Referente="e.esposito@outlook.com" },
                new Order { CustomerId=4, OrderDate=DateTime.Now.AddDays(-1), BoxCode="Fefco 1", Quantity=1200, TotalPrice=2800.00m, Status=OrderStatus.InProduction, Referente="g.esposito@outlook.com" },
                new Order { CustomerId=5, OrderDate=DateTime.Now.AddDays(-1), BoxCode="Fefco 2", Quantity=2000, TotalPrice=4000.00m, Status=OrderStatus.Pending, Referente="p.furlan@gmail.com" },
                new Order { CustomerId=6, OrderDate=DateTime.Now, BoxCode="Fefco 2", Quantity=800, TotalPrice=1600.00m, Status=OrderStatus.Pending, Referente="f.esposito@outlook.com" },
                new Order { CustomerId=8, OrderDate=DateTime.Now, BoxCode="Fefco 1", Quantity=1500, TotalPrice=3500.00m, Status=OrderStatus.InProduction, Referente="v.ferrari@outlook.com" }
            };

            foreach (Order o in orders)
            {
                context.Orders.Add(o);
            }
            context.SaveChanges();

            // Seed matching TechnicalSheets for all 15 orders
            for (int i = 0; i < orders.Length; i++)
            {
                var o = orders[i];
                var sheet = new TechnicalSheet
                {
                    OrderId = o.Id,
                    Length = 30 + (i % 3) * 5,
                    Width = 20 + (i % 2) * 5,
                    Height = 15 + (i % 4) * 2,
                    CardboardType = "Ondulato",
                    WaveType = (i % 2 == 0) ? "B" : "BC",
                    FefcoCode = o.BoxCode.StartsWith("Fefco") ? o.BoxCode.Replace("Fefco ", "0") : "0201",
                    HasPrinting = (i % 2 == 0),
                    ColorCount = (i % 2 == 0) ? 2 : 0,
                    CustomerLogoPath = "",
                    PrintingType = (i % 2 == 0) ? "Flexo" : "N/A",
                    UnitPrice = o.TotalPrice / o.Quantity,
                    Discount = 0
                };
                context.TechnicalSheets.Add(sheet);
            }
            context.SaveChanges();
        }
    }
}
