using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs.Product;

public class DeleteProductRequest
{
    [Required]
    public long Id { get; set; }
}
