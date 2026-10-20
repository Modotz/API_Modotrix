namespace ERP.Api.DTOs;

public class UpdateWorkCenterRequest
{
    public long Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public decimal CostPerHour { get; set; }

    public bool IsActive { get; set; }
}