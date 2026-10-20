using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs.GoodsPurchaseOrder;

public class DeleteGoodsPurchaseOrderRequest
{
    [Required]
    public long Id { get; set; }
}
