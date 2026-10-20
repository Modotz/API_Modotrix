using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Api.Models;
namespace ERP.Api.DTOs.GoodsPurchaseOrder;

public class GoodsPurchaseOrderDto
{
    public long Id { get; set; }

    public string? Uid { get; set; }
    public string PONo { get; set; } = string.Empty;
    public DateOnly PODate { get; set; }

    public long SupplierId { get; set; }
    public string? SupplierName { get; set; } = string.Empty;

    public long? PRId { get; set; }
    public long? RFQId { get; set; }

    public long WarehouseId { get; set; }
    public string? WarehouseName { get; set; } = string.Empty;

    public DateOnly? ExpectedDate { get; set; }
    public string? Note { get; set; }

    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal TaxAmount { get; set; }

    public string Status { get; set; } = "draft";
    public decimal TotalAmount { get; set; }

    public int? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<GoodsPurchaseOrderItemDto> Details { get; set; } = new ();
}



