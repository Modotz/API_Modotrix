
namespace ERP.Api.DTOs;

public class GoodsRfqItemProductDto

{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }

    public long? UnitId { get; set; }
    public string? UnitSymbol { get; set; }

    public decimal? CostPrice { get; set; }
}
