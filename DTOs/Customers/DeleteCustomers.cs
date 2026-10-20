using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs.Customer;

public class DeleteCustomerDto
{
    [Required]
    public long Id { get; set; }
}