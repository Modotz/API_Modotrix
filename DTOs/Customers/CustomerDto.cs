namespace ERP.Api.DTOs.Customer;

public class CustomerDto
{
    public long Id { get; set; }

    public string? Uid { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Npwp { get; set; }

    public string? Nik { get; set; }

    public string Address { get; set; } = string.Empty;

    public string? Image { get; set; }

    public long? CustomerGroupId { get; set; }

    public string? CustomerGroupName { get; set; }

    public long? SalesForceId { get; set; }

    public string? SalesForceName { get; set; }

    public long? PriceListId { get; set; }

    public string? PriceListName { get; set; }

    public long? ShippingAreaId { get; set; }

    public string? ShippingAreaName { get; set; }

    public bool? RegisteredOnline { get; set; }

    public string? ShopStatus { get; set; }

    public long? UserId { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}