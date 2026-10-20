using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs.User;

public class DeleteUserRequest
{
    [Required]
    public long Id { get; set; }
}