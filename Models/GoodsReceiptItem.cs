using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Api.Models
{
    [Table("goods_receipt_items")]
    public class GoodsReceiptItem
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("uid")]
        [MaxLength(36)]
        public string? Uid { get; set; }

        [Column("receipt_id")]
        [MaxLength(50)]
        public long ReceiptId { get; set; }

        [Column("product_id")]
        public long ProductId { get; set; }

        [NotMapped]
        public string? ProductName { get; set; }

        [Column("dim_length")]
        public decimal? DimLength { get; set; } = 0.00m;

        [Column("dim_width")]
        public decimal? DimWidth { get; set; } = 0.00m;

        [Column("dim_height")]
        public decimal? DimHeight { get; set; } = 0.00m;

        [Column("dim_count")]
        public decimal? DimCount { get; set; } = 0.00m;

        [Column("unit_id")]
        public long? UnitId { get; set; }

        [NotMapped]
        public string? UnitCode { get; set; }

        [Column("unit_qty")]
        public decimal? UnitQty { get; set; } = 0.00m;

        [Column("quantity")]
        public decimal Qty { get; set; } = 0.00m;

        [Column("unit_cost")]
        public decimal UnitCost { get; set; } = 0.00m;

        [Column("subtotal")]
        public decimal Subtotal { get; set; } = 0.00m;

    }
}