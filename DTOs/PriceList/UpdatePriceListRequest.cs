namespace ERP.Api.DTOs;

public class UpdatePriceListRequest
{
    public long Id { get; set; }

    public string? Code { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Note { get; set; }

    public bool IsActive { get; set; }
}
