using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Api.Models;

[Table("customers")]
public class Customer
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("uid")]
    [MaxLength(36)]
    public string? Uid { get; set; }

    [Required]
    [Column("full_name")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Column("email")]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Column("password")]
    [MaxLength(255)]
    public string? Password { get; set; }

    [Required]
    [Column("phone")]
    [MaxLength(13)]
    public string Phone { get; set; } = string.Empty;

    [Column("npwp")]
    [MaxLength(16)]
    public string? Npwp { get; set; }

    [Column("nik")]
    [MaxLength(16)]
    public string? Nik { get; set; }

    [Required]
    [Column("address")]
    public string Address { get; set; } = string.Empty;

    [Column("image")]
    [MaxLength(255)]
    public string? Image { get; set; }

    [Column("customer_group_id")]
    public long? CustomerGroupId { get; set; }

    [Column("sales_force_id")]
    public long? SalesForceId { get; set; }

    [Column("email_verified_at")]
    public DateTime? EmailVerifiedAt { get; set; }

    [Column("remember_token")]
    [MaxLength(64)]
    public string? RememberToken { get; set; }

    [Column("registered_online")]
    public bool? RegisteredOnline { get; set; }

    [Column("price_list_id")]
    public long? PriceListId { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [Column("user_id")]
    public long? UserId { get; set; }

    [Column("shop_status")]
    [MaxLength(20)]
    public string? ShopStatus { get; set; }

    // Navigation
    public CustomerGroups? CustomerGroup { get; set; }

    public SalesForce? SalesForce { get; set; }

    public PriceList? PriceList { get; set; }

    public ICollection<ShippingAddress> ShippingAddresses { get; set; }
        = new List<ShippingAddress>();
}