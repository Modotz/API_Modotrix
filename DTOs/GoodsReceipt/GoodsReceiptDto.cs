using ERP.Api.DTOs.GoodsReceipt;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ERP.Api.DTOs.GoodsReceipt;

public class GoodsReceiptDto
{
    public long Id { get; set; }

    public string? Uid { get; set; }
    public string ReceiptNo { get; set; } = string.Empty;
    public DateOnly ReceiptDate { get; set; } 
    public long WarehouseId { get; set; }
    public string? WarehouseName { get; set; } = string.Empty;
    public long SupplierId { get; set; }
    public string? SupplierName { get; set; } = string.Empty;

    public long? POId { get; set; }
    public long? RefNo { get; set; }
    public string? Note { get; set; }
    public string Status { get; set; } = "draft";
    public int? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<GoodsReceiptItemDto> Details { get; set; } = new();
}



