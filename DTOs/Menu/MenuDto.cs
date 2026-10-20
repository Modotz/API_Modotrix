namespace ERP.Api.DTOs.Menu;

public class MenuDto
{
    public long Id { get; set; }
    public string? Uid { get; set; }
    public long? ParentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string UrlSlug { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
