using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace ERP.Api.Models;

[Table("work_centers")]
public class WorkCenter
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("uid")]
    [MaxLength(36)]
    public string? Uid { get; set; }

    [Column("code")]
    [MaxLength(30)]
    public string Code { get; set; } = string.Empty;

    [Column("name")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column("cost_per_hour")]
    [MaxLength(191)]
    public decimal CostPerHour { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }
}