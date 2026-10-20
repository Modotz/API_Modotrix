namespace ERP.Api.DTOs.ProductUnit;

public class ProductUnitDto 
{
    public long Id { get; set; }
    public long? Uid { get; set; }
    public long ProductId { get; set; }
    public long UnitId { get; set; }
    public decimal Conversion { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    //public DateTime? DeletedAt { get; set; }
}
