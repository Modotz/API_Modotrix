using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs.Menu;

public class OrderMenuRequest
{
    public long Id { get; set; }

    public long? ParentId { get; set; }

    public int SortOrder { get; set; }

}
