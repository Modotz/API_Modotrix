namespace ERP.Api.DTOs;

public class CreatePriceListItemRequest
{
    public long PriceListId { get; set; }

    public long ProductId { get; set; }

    public decimal Price { get; set; }

    public bool? TaxInclusive { get; set; }
}

