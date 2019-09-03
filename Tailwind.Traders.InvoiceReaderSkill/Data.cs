using System;
using System.Collections.Generic;
using System.Text;

namespace Tailwind.Traders.InvoiceReaderSkill
{
    public class Company
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string StateProvince { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
    }

    public class Person
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string StateProvince { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Email { get; set; }
    }

    public class Invoice
    {
        public int InvoiceId { get; set; }
        public DateTime OrderDate { get; set; }
        public Company Company { get; set; }
        public Person Person { get; set; }
        public LineItem[] LineItems { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TotalTax { get; set; }
        public decimal Total { get; set; }
    }

    public class LineItem
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public decimal DiscountTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal LineTotal { get; set; }
    }
}
