using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs.ShippingAddress;

public class UpdateShippingAddressDto
{
    [Required]
    public long Id { get; set; }

    [Required]
    public long CustomerId { get; set; }

    [Required(ErrorMessage = "Nama alamat wajib diisi.")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Alamat wajib diisi.")]
    [MaxLength(255)]
    public string Address { get; set; } = string.Empty;

    public long? ShippingAreaId { get; set; }
}