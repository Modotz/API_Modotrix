using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs.ShippingArea;

public class CreateShippingAreaRequest
{

    [Required, MaxLength(100)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string? Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
