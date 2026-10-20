using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs.Supplier;

public class DeleteSupplierRequest
{
    [Required]
    public long Id { get; set; }
}
