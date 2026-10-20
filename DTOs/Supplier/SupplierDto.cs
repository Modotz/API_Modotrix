namespace ERP.Api.DTOs.Supplier;

public class SupplierDto
{
    public long Id { get; set; }
    public string? Uid { get; set; }
    public string Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; } = string.Empty;
    public string? Phone { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public string? Npwp { get; set; } = string.Empty;
    public string? Address { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
