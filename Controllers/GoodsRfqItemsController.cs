
using ERP.Api.Data;
using ERP.Api.DTOs;
using ERP.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GoodsRfqItemsController : ControllerBase
{
    private readonly AppDbContext _db;

    public GoodsRfqItemsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _db.GoodsRfqItems
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .ToListAsync();

        return Ok(data);
    }

    // GET: api/GoodsRfqItems/by-rfq/1
    [HttpGet("by-rfq/{rfqId:long}")]
    public async Task<IActionResult> GetByRfq(long rfqId)
    {

        var data = await _db.GoodsRfqItems
            .AsNoTracking()
            .Where(x => x.RfqId == rfqId)
            .OrderBy(x => x.Id)
            .ToListAsync();

        return Ok(data);
    }

    // GET: api/GoodsRfqItems/1
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var item = await _db.GoodsRfqItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item == null)
            return NotFound(new { message = "Item RFQ tidak ditemukan." });

        return Ok(item);
    }

    // POST: api/GoodsRfqItems
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] GoodsRfqItemDto dto,
        [FromQuery] long rfqId)
    {
        var rfq = await _db.GoodsRfqs
            .FirstOrDefaultAsync(x => x.Id == rfqId);

        if (rfq == null)
            return NotFound(new { message = "Goods RFQ tidak ditemukan." });

        if (dto.ProductId <= 0)
            return BadRequest(new { message = "Produk wajib dipilih." });

        if (dto.Quantity <= 0)
            return BadRequest(new { message = "Quantity harus lebih dari 0." });

        if (dto.UnitQty < 0 ||
            dto.DimLength < 0 ||
            dto.DimWidth < 0 ||
            dto.DimHeight < 0 ||
            dto.DimCount < 0 ||
            dto.EstPrice < 0 ||
            dto.QuotedPrice < 0)
        {
            return BadRequest(new
            {
                message = "Nilai jumlah, dimensi, dan harga tidak boleh negatif."
            });
        }

        var item = new GoodsRfqItem
        {
            RfqId = rfqId,
            ProductId = dto.ProductId,
            UnitId = dto.UnitId,
            UnitQty = dto.UnitQty,
            DimLength = dto.DimLength,
            DimWidth = dto.DimWidth,
            DimHeight = dto.DimHeight,
            DimCount = dto.DimCount,
            Quantity = dto.Quantity,
            EstPrice = dto.EstPrice,
            QuotedPrice = dto.QuotedPrice,
            Subtotal = dto.Quantity * dto.EstPrice,
            Note = dto.Note
        };

        _db.GoodsRfqItems.Add(item);

        rfq.SubtotalAmount += item.Subtotal;
        rfq.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = item.Id },
            item);
    }

    // PUT: api/GoodsRfqItems/1
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] GoodsRfqItemDto dto)
    {
        var item = await _db.GoodsRfqItems
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item == null)
            return NotFound(new { message = "Item RFQ tidak ditemukan." });

        if (dto.ProductId <= 0)
            return BadRequest(new { message = "Produk wajib dipilih." });

        if (dto.Quantity <= 0)
            return BadRequest(new { message = "Quantity harus lebih dari 0." });

        if (dto.UnitQty < 0 ||
            dto.DimLength < 0 ||
            dto.DimWidth < 0 ||
            dto.DimHeight < 0 ||
            dto.DimCount < 0 ||
            dto.EstPrice < 0 ||
            dto.QuotedPrice < 0)
        {
            return BadRequest(new
            {
                message = "Nilai jumlah, dimensi, dan harga tidak boleh negatif."
            });
        }

        var rfq = await _db.GoodsRfqs
            .FirstOrDefaultAsync(x => x.Id == item.RfqId);

        if (rfq == null)
            return NotFound(new { message = "Goods RFQ tidak ditemukan." });

        // Kurangi subtotal item lama dari total RFQ
        rfq.SubtotalAmount -= item.Subtotal;

        item.ProductId = dto.ProductId;
        item.UnitId = dto.UnitId;
        item.UnitQty = dto.UnitQty;
        item.DimLength = dto.DimLength;
        item.DimWidth = dto.DimWidth;
        item.DimHeight = dto.DimHeight;
        item.DimCount = dto.DimCount;
        item.Quantity = dto.Quantity;
        item.EstPrice = dto.EstPrice;
        item.QuotedPrice = dto.QuotedPrice;
        item.Subtotal = dto.Quantity * dto.EstPrice;
        item.Note = dto.Note;

        // Tambahkan subtotal item yang baru
        rfq.SubtotalAmount += item.Subtotal;
        rfq.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(item);
    }

    // GET: api/GoodsRfqItems/products
    [HttpGet("products")]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _db.Products
            .AsNoTracking()
            .Where(p => p.IsActive && p.DeletedAt == null)
            .OrderBy(p => p.Name)
            .Select(p => new GoodsRfqItemProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Sku = p.Sku,
                UnitId = p.UnitId,
                CostPrice = p.CostPrice
            })
            .ToListAsync();

        var unitIds = products
            .Where(p => p.UnitId.HasValue)
            .Select(p => p.UnitId!.Value)
            .Distinct()
            .ToList();

        var units = await _db.Units
            .AsNoTracking()
            .Where(u => unitIds.Contains(u.Id) && u.DeletedAt == null)
            .Select(u => new
            {
                u.Id,
                u.Symbol
            })
            .ToListAsync();

        foreach (var product in products)
        {
            product.UnitSymbol = product.UnitId.HasValue
                ? units.FirstOrDefault(u => u.Id == product.UnitId.Value)?.Symbol
                : null;
        }

        return Ok(products);
   
    }

    // DELETE: api/GoodsRfqItems/1
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var item = await _db.GoodsRfqItems
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item == null)
            return NotFound(new { message = "Item RFQ tidak ditemukan." });

        var rfq = await _db.GoodsRfqs
            .FirstOrDefaultAsync(x => x.Id == item.RfqId);

        if (rfq != null)
        {
            rfq.SubtotalAmount -= item.Subtotal;

            if (rfq.SubtotalAmount < 0)
                rfq.SubtotalAmount = 0;

            rfq.UpdatedAt = DateTime.Now;
        }

        _db.GoodsRfqItems.Remove(item);

        await _db.SaveChangesAsync();

        return Ok(new { message = "Item RFQ berhasil dihapus." });
    }
}
