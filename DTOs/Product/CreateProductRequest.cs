using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs.Product;

public class CreateProductRequest
{
    [Required, MaxLength(50)]
    public string Sku { get; set; } = string.Empty;

    public string? Barcode { get; set; }

    public string? InternalCode { get; set; }

    public string? ExternalCode1 { get; set; }

    public string? ExternalCode2 { get; set; }

    public string? ExternalCode3 { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; }

    public long? BrandId { get; set; }

    public long? CategoryId { get; set; }

    public long? SubCategoryId { get; set; }

    public string? ProductType { get; set; } = "RAW_MATERIAL";

    public long UnitId { get; set; }

    public long? ParentId { get; set; }

    public bool? HasVariants { get; set; } = false;

    public string? DimMode { get; set; } = "none";

    public decimal? DefLength { get; set; }

    public decimal? DefWidth { get; set; }

    public decimal? DefHeight { get; set; }

    public string? PriceMode { get; set; } = "fixed";

    public decimal? CostPerM3 { get; set; }

    public decimal? SellPerM3 { get; set; }

    public string? Description { get; set; }

    public decimal? CostPrice { get; set; }

    public decimal? SellPrice { get; set; }

    public decimal? MinStock { get; set; }

    public bool? TrackStock { get; set; } = true;

    public bool? TrackSerial { get; set; } = false;

    public int? WarrantyMonths { get; set; }

    public string? Image { get; set; }

    public bool? IsActive { get; set; } = true;
}