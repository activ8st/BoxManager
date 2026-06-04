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
                new Customer { BusinessName="Cartotecnica Cesenate", VatNumber="01234567890", Address="Via del Campus, 1, Cesena", Email="info@cartocesena.it", Phone="0547 123456", ContactPerson="Andrea Bianchi", Notes="Cliente storico" },
                new Customer { BusinessName="Packaging Unibo", VatNumber="09876543210", Address="Viale dell'Università, 10, Cesena", Email="ordini@packunibo.it", Phone="0547 654321", ContactPerson="Mario Rossi", Notes="Pagamento 30gg" }
            };

            foreach (Customer c in customers)
            {
                context.Customers.Add(c);
            }
            context.SaveChanges();

            var orders = new Order[]
            {
                new Order { CustomerId=1, OrderDate=DateTime.Now.AddDays(-5), BoxCode="BX-100", Quantity=500, TotalPrice=1250.00m, Status=OrderStatus.Completed },
                new Order { CustomerId=1, OrderDate=DateTime.Now.AddDays(-2), BoxCode="BX-200", Quantity=1000, TotalPrice=3000.00m, Status=OrderStatus.InProduction },
                new Order { CustomerId=2, OrderDate=DateTime.Now, BoxCode="BX-300", Quantity=200, TotalPrice=600.00m, Status=OrderStatus.Pending }
            };

            foreach (Order o in orders)
            {
                context.Orders.Add(o);
            }
            context.SaveChanges();

            var sheets = new TechnicalSheet[]
{
    new TechnicalSheet {
        OrderId=1,
        Length=30,
        Width=20,
        Height=15,
        CardboardType="Ondulato",
        WaveType="B",
        FefcoCode="0201",
        HasPrinting=true,
        ColorCount=2,
        CustomerLogoPath="",
        PrintingType="Flexo",
        UnitPrice=2.50m,
        Discount=0
    },

    new TechnicalSheet {
        OrderId=2,
        Length=40,
        Width=30,
        Height=20,
        CardboardType="Ondulato",
        WaveType="BC",
        FefcoCode="0203",
        HasPrinting=false,
        ColorCount=0,
        CustomerLogoPath="",
        PrintingType="N/A",
        UnitPrice=3.00m,
        Discount=0
    },

    new TechnicalSheet {
        OrderId=3,
        Length=25,
        Width=25,
        Height=25,
        CardboardType="Ondulato",
        WaveType="E",
        FefcoCode="0427",
        HasPrinting=true,
        ColorCount=4,
        CustomerLogoPath="",
        PrintingType="Offset",
        UnitPrice=3.00m,
        Discount=0
    }
      };

            foreach (TechnicalSheet s in sheets)
            {
                context.TechnicalSheets.Add(s);
            }
            context.SaveChanges();
        }
    }
}
