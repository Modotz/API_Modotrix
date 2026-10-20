using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs;

public class CreateWarehouse
{
    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? PicName { get; set; }

    public bool IsDefault { get; set; } = false;

    public bool IsActive { get; set; } = true;
}