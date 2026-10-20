using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Api.Models;

[Table("shipping_areas")]
public class ShippingArea
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("uid")]
    [MaxLength(36)]
    public string? Uid { get; set; }

    [Column("code")]
    public string? Code { get; set; }

    [Column("name")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column("price")]
    [MaxLength(50)]
    public decimal Price { get; set; } 

    [Column("description")]
    [MaxLength(255)]
    public string? Description { get; set; } = string.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }
}
