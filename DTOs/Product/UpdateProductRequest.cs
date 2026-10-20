using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs.Product;

public class UpdateProductRequest
{
    public long Id { get; set; }
    public string? Uid { get; set; }
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    public string? InternalCode { get; set; }
    public string? ExternalCode1 { get; set; }
    public string? ExternalCode2 { get; set; }
    public string? ExternalCode3 { get; set; }
    public string Name { get; set; } = string.Empty;
    public long? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public long? SubCategoryId { get; set; }
    public string? SubCategoryName { get; set; }
    public long? BrandId { get; set; }
    public string? BrandName { get; set; }
    public string ProductType { get; set; } = "RAW_MATERIAL";
    public long? ParentId { get; set; }
    public bool? HasVariants { get; set; } = false;
    public long UnitId { get; set; }
    public string? UnitName { get; set; }
    public string? DimMode { get; set; }
    public decimal? DefLength { get; set; }
    public decimal? DefWidth { get; set; }
    public decimal? DefHeight { get; set; }
    public string? PriceMode { get; set; }
    public decimal? CostPerM3 { get; set; }
    public decimal? SellPerM3 { get; set; }
    public string? Description { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellPrice { get; set; }
    public decimal MinStock { get; set; }
    public bool TrackStock { get; set; }
    public bool? TrackSerial { get; set; } = false;
    public int? WarrantyMonths { get; set; }
    public bool IsActive { get; set; }
    public string? Image { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

}
