namespace ERP.Api.DTOs;

public class RoleDto
{
    public long Id { get; set; }
    public string? Uid { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
