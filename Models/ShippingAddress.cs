using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Api.Models;

[Table("shipping_addresses")]
public class ShippingAddress
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("uid")]
    [MaxLength(40)]
    public string? Uid { get; set; }

    [Required]
    [Column("customer_id")]
    public long CustomerId { get; set; }

    [Required]
    [Column("name")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Column("address")]
    [MaxLength(255)]
    public string Address { get; set; } = string.Empty;

    [Column("shipping_area_id")]
    public long? ShippingAreaId { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Customer? Customer { get; set; }

    public ShippingArea? ShippingArea { get; set; }
}