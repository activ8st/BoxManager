using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoxManager.Models
{
    public enum OrderStatus
    {
        [Display(Name = "In attesa")]
        Pending,
        [Display(Name = "In corso")]
        InProduction,
        [Display(Name = "Pronto")]
        Completed,
        [Display(Name = "Consegnato")]
        Delivered,
        [Display(Name = "Annullato")]
        Cancelled
    }

    public class Customer
    {
        public int Id { get; set; }
        [Required, Display(Name = "Ragione Sociale")]
        public string BusinessName { get; set; }
        [Required, Display(Name = "Partita IVA")]
        public string VatNumber { get; set; }
        [Required, Display(Name = "Indirizzo")]
        public string Address { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required, Display(Name = "Telefono")]
        public string Phone { get; set; }
        [Required, Display(Name = "Referente")]
        public string ContactPerson { get; set; }
        [Display(Name = "Note")]
        public string Notes { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }

    public class Order
    {
        public int Id { get; set; }
        [Required, Display(Name = "Data Ordine")]
        public DateTime OrderDate { get; set; } = DateTime.Now;
        [Required, Display(Name = "Codice Scatola")]
        public string BoxCode { get; set; }
        [Required, Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Display(Name = "Referente")]
        public string Referente { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public TechnicalSheet TechnicalSheet { get; set; }
    }

    public class TechnicalSheet
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }

        // Struttura
        [Required, Display(Name = "Lunghezza (L)")]
        public double Length { get; set; }
        [Required, Display(Name = "Larghezza (W)")]
        public double Width { get; set; }
        [Required, Display(Name = "Altezza (H)")]
        public double Height { get; set; }
        [Required, Display(Name = "Tipologia Cartone")]
        public string CardboardType { get; set; }
        [Required, Display(Name = "Tipo Onda")]
        public string WaveType { get; set; }
        [Required, Display(Name = "Codice FEFCO")]
        public string FefcoCode { get; set; }

        // Stampa
        [Display(Name = "Presenza Stampa")]
        public bool HasPrinting { get; set; }
        [Display(Name = "Numero Colori")]
        public int ColorCount { get; set; }
        [Display(Name = "Logo Cliente")]
        public string CustomerLogoPath { get; set; }
        [Display(Name = "Tipologia Stampa")]
        public string PrintingType { get; set; } // offset/flexo
        [Display(Name = "Finiture Speciali")]
        public string SpecialFinishes { get; set; }

        // Aspetti economici
        [Required, Column(TypeName = "decimal(18,2)"), Display(Name = "Prezzo Unitario")]
        public decimal UnitPrice { get; set; }
        [Required, Range(0, 100), Display(Name = "Sconto (%)")]
        public decimal Discount { get; set; }
    }
}
