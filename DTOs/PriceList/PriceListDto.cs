namespace ERP.Api.DTOs;

public class PriceListDto
{
    public long Id { get; set; }

    public string? Uid { get; set; }

    public string? Code { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Note { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}