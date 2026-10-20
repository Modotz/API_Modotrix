using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Api.Models
{
    [Table("goods_purchase_order_items")]
    public class GoodsPurchaseOrderItem
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("uid")]
        [MaxLength(36)]
        public string? Uid { get; set; }

        [Column("po_id")]
        [MaxLength(50)]
        public long POId { get; set; } 

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

        [Column("received_qty")]
        public decimal ReceivedQty { get; set; } = 0.00m;

        [Column("unit_price")]
        public decimal UnitPrice { get; set; } = 0.00m;

        [Column("disc_percent")]
        public decimal? DiscPercent { get; set; } = 0.00m;

        [Column("tax_percent")]
        public decimal? TaxPercent { get; set; } = 0.00m;

        [Column("subtotal")]
        public decimal Subtotal { get; set; } = 0.00m;

    }
}