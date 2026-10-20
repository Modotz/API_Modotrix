using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs.ProductUnit;

public class UpdateProductUnitRequest
{

    public long ProductId { get; set; }

    public long UnitId { get; set; }

    public decimal Conversion { get; set; }

}
