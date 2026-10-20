using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Api.Models;

[Table("goods_rfqs")]
public class GoodsRfq
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("uid")]
    [MaxLength(36)]
    public string? Uid { get; set; }

    [Column("rfq_no")]
    [MaxLength(50)]
    public string RfqNo { get; set; } = string.Empty;

    [Column("rfq_date")]
    public DateTime RfqDate { get; set; }

    [Column("valid_until")]
    public DateTime? ValidUntil { get; set; }

    [Column("supplier_id")]
    public long SupplierId { get; set; }

    [Column("pr_id")]
    public long? PrId { get; set; }

    [Column("note")]
    public string? Note { get; set; }

    [Column("status")]
    [MaxLength(20)]
    public string Status { get; set; } = "draft";

    [Column("subtotal_amount")]
    public decimal SubtotalAmount { get; set; }

    [Column("created_by")]
    public long? CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}