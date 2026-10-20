using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Api.Models;

[Table("msfields")]
public class Msfield
{
    [Key]
    [Column("id")]
    public long id { get; set; }

    [Column("uid")]
    [MaxLength(36)]
    public string? uid { get; set; }

    [Column("field_key")]
    [MaxLength(50)]
    public string field_key { get; set; } = string.Empty;

    [Column("field_text")]
    [MaxLength(255)]
    public string field_text { get; set; } = string.Empty;

    [Column("filed_value")]
    [MaxLength(255)]
    public string filed_value { get; set; } = string.Empty;

    [Column("description")]
    public string? description { get; set; }

    [Column("is_active")]
    public bool is_active { get; set; } = true;

    [Column("created_at")]
    public DateTime? created_at { get; set; }

    [Column("updated_at")]
    public DateTime? updated_at { get; set; }

    [Column("deleted_at")]
    public DateTime? deleted_at { get; set; }

    [Column("user_id")]
    public int user_id { get; set; }
}