using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoomRentalManagerServer.Domain.ModelEntities.Invoices
{
    [Table("invoice_item")]
    public class InvoiceItem
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("invoiceId")]
        public long InvoiceId { get; set; }

        [Column("itemType")]
        public InvoiceItemType ItemType { get; set; }

        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [Column("quantity")]
        public decimal Quantity { get; set; }

        [Column("unitPrice")]
        public decimal UnitPrice { get; set; }

        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("sortOrder")]
        public int SortOrder { get; set; }
    }

    public enum InvoiceItemType
    {
        Rent = 1,
        Electric = 2,
        Water = 3,
        Garbage = 4
    }
}
