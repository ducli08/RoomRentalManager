using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Domain.ModelEntities.Invoices
{
    [Table("invoice")]
    public class Invoice
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("contractId")]
        public long ContractId { get; set; }

        [Column("invoiceDate")]
        public DateTime InvoiceDate { get; set; }

        [Column("dueDate")]
        public DateTime DueDate { get; set; }

        [Column("totalAmount")]
        public decimal TotalAmount { get; set; }

        [Column("amountPaid")]
        public decimal AmountPaid { get; set; }

        [Column("invoiceStatus")]
        public InvoiceStatus Status { get; set; }

        [Column("issuedAt")]
        public DateTime? IssuedAt { get; set; }

        [Column("cancelledAt")]
        public DateTime? CancelledAt { get; set; }

        [Column("createdAt")]
        public DateTime CreatedAt { get; set; }

        [Column("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [Column("creatorUser")]
        public string CreatorUser { get; set; }

        [Column("lastUpdateUser")]
        public string LastUpdateUser { get; set; }

    }
    public enum InvoiceStatus
    {
        [Display(Name = "Nháp")]
        Draft = 1,
        [Display(Name = "Đã phát hành")]
        Issued = 2,
        [Display(Name = "Đã thanh toán một phần")]
        PartiallyPaid = 3,
        [Display(Name = "Đã thanh toán")]
        Paid = 4,
        [Display(Name = "Đã hủy")]
        Cancelled = 5
    }
}
