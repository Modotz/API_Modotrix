using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs.Supplier;

public class CreateSupplierRequest
{

    [Required, MaxLength(100)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? ContactPerson { get; set; } = string.Empty;

    public string? Phone { get; set; } = string.Empty;

    public string? Email { get; set; } = string.Empty;

    public string? Npwp { get; set; } = string.Empty;

    public string? Address { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
