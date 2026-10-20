using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs.ProductUnit;

public class CreateProductUnitRequest
{

    [Required]
    public long ParentId { get; set; }

    [Required]
    public long UnitId { get; set; }

    public decimal Conversion { get; set; }

}
