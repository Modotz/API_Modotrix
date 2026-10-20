namespace ERP.Api.DTOs;

public class CreateGoodsRfqDto
{
    public DateTime RfqDate { get; set; } = DateTime.Today;
    public DateTime? ValidUntil { get; set; }

    public long SupplierId { get; set; }
    public long? PrId { get; set; }
    public string? Note { get; set; }

    public List<GoodsRfqItemDto> Items { get; set; } = new();
}
