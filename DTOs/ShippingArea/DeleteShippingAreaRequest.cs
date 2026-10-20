using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs.ShippingArea;

public class DeleteShippingAreaRequest
{
    [Required]
    public long Id { get; set; }
}
