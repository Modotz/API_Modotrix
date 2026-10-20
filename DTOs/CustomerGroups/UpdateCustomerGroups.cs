namespace ERP.Api.DTOs;

public class UpdateCustomerGroups
{
    public long Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public decimal DiscountPercent { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public long? PriceListId { get; set; }
}