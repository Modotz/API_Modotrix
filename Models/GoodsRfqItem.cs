using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ERP.Api.Models;

[Table("goods_rfq_items")]
public class GoodsRfqItem
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("rfq_id")]
    public long RfqId { get; set; }

    [Column("product_id")]
    public long ProductId { get; set; }

    [Column("unit_id")]
    public long? UnitId { get; set; }

    [Column("unit_qty", TypeName = "decimal(18,3)")]
    public decimal UnitQty { get; set; }

    [Column("dim_length", TypeName = "decimal(12,3)")]
    public decimal DimLength { get; set; }

    [Column("dim_width", TypeName = "decimal(12,3)")]
    public decimal DimWidth { get; set; }

    [Column("dim_height", TypeName = "decimal(12,3)")]
    public decimal DimHeight { get; set; }

    [Column("dim_count", TypeName = "decimal(12,3)")]
    public decimal DimCount { get; set; }

    [Column("quantity", TypeName = "decimal(18,3)")]
    public decimal Quantity { get; set; }

    [Column("est_price", TypeName = "decimal(18,2)")]
    public decimal EstPrice { get; set; }

    [Column("quoted_price", TypeName = "decimal(18,2)")]
    public decimal QuotedPrice { get; set; }

    [Column("subtotal", TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    [Column("note")]
    [MaxLength(255)]
    public string? Note { get; set; }
}

