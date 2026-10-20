using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs.GoodsPurchaseOrder;

public class CreateGoodsPurchaseOrderRequest
{

    public string PONo { get; set; } = string.Empty;

    public DateOnly PODate { get; set; }

    public long SupplierId { get; set; }

    public long? PRId { get; set; }

    public long? RFQId { get; set; }

    public long WarehouseId { get; set; }

    public DateOnly? ExpectedDate { get; set; }

    public string? Note { get; set; }

    public decimal? SubtotalAmount { get; set; } = 0.00m;

    public decimal? DiscountAmount { get; set; } = 0.00m;

    public decimal? TaxPercent { get; set; } = 0.00m;

    public decimal? TaxAmount { get; set; } = 0.00m;

    public string Status { get; set; } = "draft";

    public decimal TotalAmount { get; set; } = 0.00m;

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
