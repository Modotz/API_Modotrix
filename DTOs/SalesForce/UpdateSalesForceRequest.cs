namespace ERP.Api.DTOs;

public class UpdateSalesForceRequest
{
    public long Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Area { get; set; }

    public bool IsActive { get; set; }
}