namespace ERP.Api.DTOs;

public class GoodsRfqItemDto
{
        public long Id { get; set; }
        public long RfqId { get; set; }
        public long ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? Sku { get; set; }

        public long? UnitId { get; set; }
        public string? UnitSymbol { get; set; }

        public decimal UnitQty { get; set; }
        public decimal DimLength { get; set; }
        public decimal DimWidth { get; set; }
        public decimal DimHeight { get; set; }
        public decimal DimCount { get; set; }
        public decimal Quantity { get; set; }

        public decimal EstPrice { get; set; }
        public decimal QuotedPrice { get; set; }
        public decimal Subtotal { get; set; }
        public string? Note { get; set; }
    }

// Body request untuk endpoint POST api/GoodsRfqs/{id}/quote
public class QuoteGoodsRfqDto
{
    public List<QuoteGoodsRfqItemDto> Items { get; set; } = new();
}

public class QuoteGoodsRfqItemDto
{
    public long Id { get; set; }
    public decimal QuotedPrice { get; set; }
}