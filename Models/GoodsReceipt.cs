using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Api.Models
{
    [Table("goods_receipts")]
    public class GoodsReceipt
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("uid")]
        [MaxLength(36)]
        public string? Uid { get; set; }

        [Column("receipt_no")]
        [MaxLength(50)]
        public string ReceiptNo { get; set; } = string.Empty;

        [Column("receipt_date")]
        public DateOnly ReceiptDate { get; set; }

        [Column("warehouse_id")]
        public long WarehouseId { get; set; }
         
        [NotMapped]
        public string? WarehouseName { get; set; }

        [Column("supplier_id")]
        public long SupplierId { get; set; }
         
        [NotMapped]
        public string? SupplierName { get; set; }

        [Column("po_id")]
        public long? POId { get; set; }

        [Column("ref_no")]
        public long? RefNo { get; set; }

        [Column("note")]
        public string Note { get; set; } = string.Empty;

        [Column("status")]
        [MaxLength(20)]
        public string Status { get; set; } = "draft";

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
