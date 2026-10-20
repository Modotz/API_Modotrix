using System.ComponentModel.DataAnnotations;

namespace ERP.Api.DTOs.GoodsReceipt;

public class DeleteGoodsReceiptRequest
{
    [Required]
    public long Id { get; set; }
}
