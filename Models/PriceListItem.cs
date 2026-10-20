using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Api.Models;

[Table("price_list_items")]
public class PriceListItem
{
    public long Id { get; set; }

    public string? Uid { get; set; }

    public long PriceListId { get; set; }

    public long ProductId { get; set; }

    public long? UnitId { get; set; }

    public decimal Price { get; set; } = 0.00m;

    public bool? TaxInclusive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}