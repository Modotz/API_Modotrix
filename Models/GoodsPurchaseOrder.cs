using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Api.Models
{
    [Table("goods_purchase_orders")]
    public class GoodsPurchaseOrder
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("uid")]
        [MaxLength(36)]
        public string? Uid { get; set; }

        [Column("po_no")]
        [MaxLength(50)]
        public string PONo { get; set; } = string.Empty;

        [Column("po_date")]
        public DateOnly PODate { get; set; }

        [Column("supplier_id")]
        public long SupplierId { get; set; }
         
        [NotMapped]
        public string? SupplierName { get; set; }

        [Column("pr_id")]
        public long? PRId { get; set; }

        [Column("rfq_id")]
        public long? RFQId { get; set; }

        [Column("warehouse_id")]
        public long WarehouseId { get; set; }
         
        [NotMapped]
        public string? WarehouseName { get; set; }

        [Column("expected_date")]
        public DateOnly? ExpectedDate { get; set; }

        [Column("note")]
        public string? Note { get; set; }

        [Column("subtotal_amount")]
        public decimal? SubtotalAmount { get; set; } = 0m;

        [Column("discount_amount")]
        public decimal? DiscountAmount { get; set; } = 0m;

        [Column("tax_percent")]
        public decimal? TaxPercent { get; set; } = 0m;

        [Column("tax_amount")]
        public decimal? TaxAmount { get; set; } = 0m;

        [Column("status")]
        [MaxLength(20)]
        public string Status { get; set; } = "draft";

        [Column("total_amount")]
        public decimal TotalAmount { get; set; } = 0m;

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
