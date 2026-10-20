using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Api.Models;

[Table("product_units")]
public class ProductUnit
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("uid")]
    [MaxLength(36)]
    public string? Uid { get; set; }

    [Column("product_id")]
    public long? ProductId { get; set; }

    [Column("unit_id")]
    public long? UnitId { get; set; }

    [Column("conversion")]
    [MaxLength(100)]
    public decimal Conversion { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

}
