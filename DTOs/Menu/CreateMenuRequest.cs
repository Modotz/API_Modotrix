using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs.Menu;

public class CreateMenuRequest
{
    public long? ParentId { get; set; }

    [Required, MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    public string UrlSlug { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public int SortOrder { get; set; } 

    public bool IsActive { get; set; } = true;
}
