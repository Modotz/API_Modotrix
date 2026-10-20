using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs.Supplier;

public class UpdateSupplierRequest
{
    public long Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? ContactPerson { get; set; } = string.Empty;

    public string? Phone { get; set; } = string.Empty;

    public string? Email { get; set; } = string.Empty;

    public string? Npwp { get; set; } = string.Empty;

    public string? Address { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
