using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs.Unit;

public class DeleteUnitRequest
{
    [Required]
    public long Id { get; set; }
}
