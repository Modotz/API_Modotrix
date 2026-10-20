using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs.GoodsReceipt;

public class DeleteGoodsReceiptItemRequest
{
    [Required]
    public long Id { get; set; }
}
