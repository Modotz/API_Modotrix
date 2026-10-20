using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs.GoodsPurchaseOrder;


public class DeleteGoodsPurchaseOrderItemRequest
{
    [Required]
    public long Id { get; set; }
}
