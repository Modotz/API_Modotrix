using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs.Menu;

public class DeleteMenuRequest
{
    [Required]
    public long Id { get; set; }
}
