using System.ComponentModel.DataAnnotations;

namespace RoomRentalManagerServer.Application.Model.InvoicesModel.Dto
{
    public class CreateOrEditInvoiceDto
    {
        public long? Id { get; set; }

        [Required]
        public long ContractId { get; set; }

        [Required]
        public DateTime InvoiceDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }
    }
}

