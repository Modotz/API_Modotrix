using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs;

public class UpdateMsfieldRequest
{
    [Required]
    public long Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string FieldKey { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string FieldText { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string FiledValue { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public int UserId { get; set; }
}