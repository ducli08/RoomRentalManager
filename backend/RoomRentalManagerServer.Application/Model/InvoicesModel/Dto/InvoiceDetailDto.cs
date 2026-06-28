using RoomRentalManagerServer.Application.Model.PaymentsModel.Dto;
using RoomRentalManagerServer.Domain.ModelEntities.Invoices;

namespace RoomRentalManagerServer.Application.Model.InvoicesModel.Dto
{
    public enum InvoiceDetailViewer
    {
        Admin = 1,
        Tenant = 2
    }

    public class InvoiceDetailDto
    {
        public long Id { get; set; }
        public string InvoiceCode { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int OccupancyDays { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public List<InvoiceItemDto> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceDue { get; set; }
        public InvoiceStatus Status { get; set; }
        public string DisplayStatus { get; set; } = string.Empty;
        public bool IsOverdue { get; set; }
        public int DaysLate { get; set; }
        public PaymentDto? ActivePayment { get; set; }
        public PaymentDto? LastRejectedPayment { get; set; }
        public string? QrImageUrl { get; set; }
        public long? ContractId { get; set; }
        public long? UtilityReadingId { get; set; }
        public List<PaymentDto>? PaymentHistory { get; set; }
        public InvoiceDetailActionsDto Actions { get; set; } = new();
    }

    public class InvoiceItemDto
    {
        public InvoiceItemType ItemType { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public int SortOrder { get; set; }
    }

    public class InvoiceDetailActionsDto
    {
        public bool CanPayQr { get; set; }
        public bool CanPayCash { get; set; }
        public bool CanViewQr { get; set; }
        public bool CanUploadEvidence { get; set; }
        public bool CanCancelQr { get; set; }
        public bool CanViewEvidence { get; set; }
        public bool CanApprove { get; set; }
        public bool CanReject { get; set; }
        public bool CanCancelInvoice { get; set; }
    }
}
