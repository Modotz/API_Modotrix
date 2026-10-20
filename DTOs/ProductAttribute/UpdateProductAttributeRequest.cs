namespace ERP.Api.DTOs;

public class UpdateProductAttributeRequest
{
    public long Id { get; set; }

    public string? Code { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}