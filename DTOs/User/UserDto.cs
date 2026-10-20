namespace ERP.Api.DTOs;

public class UserDto
{
    public long Id { get; set; }
    public string? Uid { get; set; }
    public long RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Photo { get; set; }
    public string? Signature { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLogin { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
