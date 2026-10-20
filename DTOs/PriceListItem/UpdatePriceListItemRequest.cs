namespace ERP.Api.DTOs;

public class UpdatePriceListItemRequest
{
    public long Id { get; set; }

    public decimal Price { get; set; }

    public bool? TaxInclusive { get; set; }
}
