using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs.ShippingArea;

public class UpdateShippingAreaRequest
{
    public long Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string? Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
