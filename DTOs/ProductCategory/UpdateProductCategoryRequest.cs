namespace ERP.Api.DTOs;

public class UpdateProductCategoryRequest
{
    public long Id { get; set; }

    public long? ParentId { get; set; }

    public string? Code { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}