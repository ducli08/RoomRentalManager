using System.ComponentModel.DataAnnotations;

namespace RoomRentalManagerServer.Application.Model.PaymentSubmissionsModel.Dto
{
    public class RejectPaymentSubmissionDto
    {
        [Required]
        public string Reason { get; set; }
    }
}

