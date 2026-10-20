using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs.Unit;

public class DeleteProductUnitRequest
{
    [Required]
    public long Id { get; set; }
}
