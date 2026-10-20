using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs.Unit;

public class CreateUnitRequest
{

    [Required, MaxLength(100)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Symbol { get; set; } = string.Empty;

    public string? Description { get; set; } = string.Empty;

}
