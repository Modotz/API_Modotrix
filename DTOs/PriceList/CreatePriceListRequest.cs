namespace ERP.Api.DTOs;

public class CreatePriceListRequest
{
    public string? Code { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Note { get; set; }

    public bool IsActive { get; set; } = true;
}