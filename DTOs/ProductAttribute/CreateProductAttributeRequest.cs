namespace ERP.Api.DTOs;

public class CreateProductAttributeRequest
{
    public string? Code { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}