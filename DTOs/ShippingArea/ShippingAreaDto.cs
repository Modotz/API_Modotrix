namespace ERP.Api.DTOs.ShippingArea;

public class ShippingAreaDto
{
    public long Id { get; set; }
    public string? Uid { get; set; }
    public string Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
