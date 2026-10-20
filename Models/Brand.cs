using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Api.Models;

[Table("brands")]
public class Brand
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("uid")]
    [StringLength(36)]
    public string? Uid { get; set; }

    [Required]
    [Column("name")]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Column("image")]
    [StringLength(255)]
    public string? Image { get; set; }

    [Required]
    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }
}