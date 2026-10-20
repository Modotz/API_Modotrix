using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs.GoodsPurchaseOrder;

public class UpdateGoodsPurchaseOrderItemRequest
{
    public string? Uid { get; set; }
    public long POId { get; set; }

    public long ProductId { get; set; }

    public decimal? DimLength { get; set; } = 0.00m;

    public decimal? DimWidth { get; set; } = 0.00m;

    public decimal? DimHeight { get; set; } = 0.00m;

    public decimal? DimCount { get; set; } = 0.00m;

    public long? UnitId { get; set; }

    public decimal? UnitQty { get; set; } = 0.00m;

    public decimal Qty { get; set; } = 0.00m;

    public decimal ReceivedQty { get; set; } = 0.00m;

    public decimal UnitPrice { get; set; } = 0.00m;

    public decimal? DiscPercent { get; set; } = 0.00m;

    public decimal Subtotal { get; set; } = 0.00m;

}
