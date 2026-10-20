using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Api.Models;

[Table("price_lists")]
public class PriceList
{
    public long Id { get; set; }

    public string? Uid { get; set; }

    public string? Code { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Note { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
