namespace ERP.Api.DTOs.Product;

public class ProductCategoryDto
{
    public long Id { get; set; }

    public string? Uid { get; set; }

    public string? ParentId{ get; set; }

    public string? Code { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}