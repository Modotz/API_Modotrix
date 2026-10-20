namespace ERP.Api.DTOs;

public class CreateProductVariantRequest
{
    public long AttributeId { get; set; }

    public string? Code { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal? DefHeight { get; set; }

    public decimal? DefWidth { get; set; }

    public decimal? DefLength { get; set; }

    public string? Description { get; set; }

    public int SortOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;
}