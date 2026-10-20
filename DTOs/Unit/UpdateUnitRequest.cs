using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs.Unit;

public class UpdateUnitRequest
{
    public long Id { get; set; }

    public string Code { get; set; } 

    public string Name { get; set; } = string.Empty;

    public string? Symbol { get; set; } = string.Empty;

    public string? Description { get; set; } = string.Empty;

}
