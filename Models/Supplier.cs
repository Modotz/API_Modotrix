using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Api.Models;

[Table("suppliers")]
public class Supplier
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

    [Column("contact_person")]
    [MaxLength(100)]
    public string? ContactPerson { get; set; } = string.Empty;

    [Column("phone")]
    [MaxLength(255)]
    public string? Phone { get; set; } = string.Empty;

    [Column("email")]
    [MaxLength(191)]
    public string? Email { get; set; } = string.Empty;

    [Column("npwp")]
    [MaxLength(50)]
    public string? Npwp { get; set; } = string.Empty;

    [Column("address")]
    [MaxLength(255)]
    public string? Address { get; set; } = string.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }
}
