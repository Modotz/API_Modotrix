using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs.Customer;

public class CreateCustomerDto 
{
    [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email wajib diisi.")]
    [EmailAddress(ErrorMessage = "Format email tidak valid.")]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Nomor telepon wajib diisi.")]
    [MaxLength(13)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(16)]
    public string? Npwp { get; set; }

    [MaxLength(16)]
    public string? Nik { get; set; }

    [Required(ErrorMessage = "Alamat wajib diisi.")]
    public string Address { get; set; } = string.Empty;

    public long? CustomerGroupId { get; set; }

    public long? SalesForceId { get; set; }

    public long? PriceListId { get; set; }

    public bool? RegisteredOnline { get; set; }

    [MaxLength(20)]
    public string? ShopStatus { get; set; }

    public long? UserId { get; set; }
}