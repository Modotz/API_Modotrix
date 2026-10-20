using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Api.Models;

[Table("warehouses")]
public class Warehouse
{
    [Key]
    [Column("id")]
    public long id { get; set; }

    [Column("uid")]
    [MaxLength(36)]
    public string? uid { get; set; }

    [Column("code")]
    [MaxLength(20)]
    public string code { get; set; } = string.Empty;

    [Column("name")]
    [MaxLength(100)]
    public string name { get; set; } = string.Empty;

    [Column("address")]
    public string? address { get; set; }

    [Column("phone")]
    [MaxLength(30)]
    public string? phone { get; set; }

    [Column("pic_name")]
    [MaxLength(100)]
    public string? pic_name { get; set; }

    [Column("is_default")]
    public bool is_default { get; set; } = false;

    [Column("is_active")]
    public bool is_active { get; set; } = true;

    [Column("created_at")]
    public DateTime? created_at { get; set; }

    [Column("updated_at")]
    public DateTime? updated_at { get; set; }

    [Column("deleted_at")]
    public DateTime? deleted_at { get; set; }
}