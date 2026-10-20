using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs.ShippingAddress;

public class DeleteShippingAddressDto
{
    [Required]
    public long Id { get; set; }
}