namespace ERP.Api.DTOs;

public class PriceListItemDto
{
    public long Id { get; set; }

    public long PriceListId { get; set; }

    public long ProductId { get; set; }

    public long? UnitId { get; set; }

    public decimal Price { get; set; }

    public bool? TaxInclusive { get; set; }

    // Data produk
    public string? ProductNumber { get; set; }

    public string? ProductName { get; set; }

    public string? Sku { get; set; }

    public string? CategoryName { get; set; }

    public string? BrandName { get; set; }

    public string? ProductType { get; set; }

    public string? UnitName { get; set; }

    // Harga default produk
    public decimal? CostPrice { get; set; }

    public decimal? SellPrice { get; set; }
}