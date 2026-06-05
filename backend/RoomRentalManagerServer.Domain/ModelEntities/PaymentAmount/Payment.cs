using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoomRentalManagerServer.Domain.ModelEntities.PaymentAmount
{
    [Table("payment")]
    public class Payment
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("invoiceId")]
        public long InvoiceId { get; set; }

        [Column("AmountPaid")]
        [Display(Name = "Số tiền đã thanh toán")]
        public decimal AmountPaid { get; set; }

        [Column("paymentDate")]
        [Display(Name = "Ngày thanh toán")]
        public DateTime PaymentDate { get; set; }

        [Column("paymentMethod")]
        [Display(Name = "Phương thức thanh toán")]
        public PaymentMethod PaymentMethod { get; set; }

        [Column("note")]
        [Display(Name = "Ghi chú")]
        public string? Note { get; set; }

        [Column("createdAt")]
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; }

        [Column("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [Column("creatorUser")]
        public string CreatorUser { get; set; }

        [Column("lastUpdateUser")]
        public string LastUpdateUser { get; set; }
    }

    public enum PaymentMethod
    {
        [Display(Name = "Tiền mặt")]
        Cash = 1,
        [Display(Name = "Chuyển khoản")]
        BankTransfer = 2,
        [Display(Name = "Thẻ")]
        Card = 3
    }
}
