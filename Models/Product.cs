using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Api.Models;

[Table("products")]
public class Product
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("uid")]
    [MaxLength(36)]
    public string? Uid { get; set; }

    [Column("product_number")]
    public string? ProductNumber { get; set; }

    [Column("sku")]
    [MaxLength(100)]
    public string Sku { get; set; } = string.Empty;

    [Column("barcode")]
    [MaxLength(50)]
    public string Barcode { get; set; } = string.Empty;

    [Column("internal_code")]
    [MaxLength(191)]
    public string InternalCode { get; set; } = string.Empty;

    [Column("external_code1")]
    [MaxLength(191)]
    public string? ExternalCode1 { get; set; }

    [Column("external_code2")]
    [MaxLength(191)]
    public string? ExternalCode2 { get; set; }

    [Column("external_code3")]
    [MaxLength(191)]
    public string? ExternalCode3 { get; set; }

    [Column("name")]
    [MaxLength(191)]
    public string Name { get; set; } = string.Empty;

    [Column("category_id")]
    [MaxLength(191)]
    public long? CategoryId { get; set; }

    [Column("sub_category_id")]
    [MaxLength(191)]
    public long? SubCategoryId { get; set; }

    [Column("brand_id")]
    [MaxLength(191)]
    public long? BrandId { get; set; }

    [Column("product_type")]
    [MaxLength(191)]
    public string ProductType { get; set; } = "RAW_MATERIAL";

    [Column("parent_id")]
    [MaxLength(191)]
    public long? ParentId { get; set; }

    [Column("has_variants")]
    [MaxLength(191)]
    public bool HasVariants { get; set; }

    [Column("unit_id")]
    [MaxLength(191)]
    public long UnitId { get; set; }

    [Column("dim_mode")]
    [MaxLength(191)]
    public string? DimMode { get; set; }

    [Column("def_length")]
    [MaxLength(191)]
    public decimal? DefLength { get; set; }

    [Column("def_width")]
    [MaxLength(191)]
    public decimal? DefWidth { get; set; }

    [Column("def_height")]
    [MaxLength(191)]
    public decimal? DefHeight { get; set; }

    [Column("price_mode")]
    [MaxLength(191)]
    public string? PriceMode { get; set; }

    [Column("cost_per_m3")]
    [MaxLength(191)]
    public decimal? CostPerM3 { get; set; }

    [Column("sell_per_m3")]
    [MaxLength(191)]
    public decimal? SellPerM3 { get; set; }

    [Column("description")]
    [MaxLength(191)]
    public string? Description { get; set; }

    [Column("cost_price")]
    [MaxLength(191)]
    public decimal? CostPrice { get; set; }

    [Column("sell_price")]
    [MaxLength(191)]
    public decimal? SellPrice { get; set; }

    [Column("min_stock")]
    [MaxLength(191)]
    public decimal? MinStock { get; set; }

    [Column("track_stock")]
    public bool TrackStock { get; set; }

    [Column("track_serial")]
    public bool TrackSerial { get; set; }

    [Column("warranty_months")]
    public int? WarrantyMonths { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("image")]
    public string? Image { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }
}
