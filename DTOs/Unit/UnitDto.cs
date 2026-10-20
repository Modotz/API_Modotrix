namespace ERP.Api.DTOs.Unit;

public class UnitDto
{
    public long Id { get; set; }
    public string? Uid { get; set; }
    public string Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
