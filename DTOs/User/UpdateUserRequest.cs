using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs;

public class UpdateUserRequest
{
    [Required]
    public long Id { get; set; }

    [Required]
    public long RoleId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(191)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Password { get; set; }

    [MaxLength(191)]
    public string? Photo { get; set; }

    public string? Signature { get; set; }

    public bool IsActive { get; set; }
}
