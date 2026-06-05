using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoomRentalManagerServer.Domain.ModelEntities.BankAccounts
{
    [Table("bank_account")]
    public class BankAccount
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("bankCode")]
        public string BankCode { get; set; }

        [Column("accountNumber")]
        public string AccountNumber { get; set; }

        [Column("accountName")]
        public string AccountName { get; set; }

        [Column("isActive")]
        public bool IsActive { get; set; }

        [Column("createdAt")]
        public DateTime CreatedAt { get; set; }

        [Column("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [Column("creatorUser")]
        public string CreatorUser { get; set; }

        [Column("lastUpdateUser")]
        public string LastUpdateUser { get; set; }
    }
}

