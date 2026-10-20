using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ERP.Api.DTOs.Brand;

public class CreateBrands
{
    [Required(ErrorMessage = "Nama brand wajib diisi.")]
    [MaxLength(50, ErrorMessage = "Nama brand maksimal 50 karakter.")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }
}