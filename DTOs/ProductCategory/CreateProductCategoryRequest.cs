namespace ERP.Api.DTOs;

public class CreateProductCategoryRequest
{
    public long? ParentId { get; set; }

    public string? Code { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}